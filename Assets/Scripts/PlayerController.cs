using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private NetworkCharacterControllerPrototype networkCharacterController = null;

    [SerializeField]
    private float moveSpeed = 15f;

    [SerializeField]
    private Bullet bulletPrefab;

    [Networked]
    public NetworkButtons ButtonsPrevious { get; set; } //record last pressed button status

    [Networked(OnChanged = nameof(OnHpChanged))] //once hp changed, operate function OnHpChanged
    public int Hp { get; set; }

    [SerializeField]
    private int maxHp = 100;

    [SerializeField]
    private Image hpBar = null;

    [SerializeField]
    private MeshRenderer meshRenderer = null;
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            Hp = maxHp;
    }
    public override void FixedUpdateNetwork() //update network stat each tick
    {
        if (GetInput(out NetworkInputData data))
        {
            NetworkButtons buttons = data.buttons;
            var pressed = buttons.GetPressed(ButtonsPrevious); //compared with the button we just pressed
            ButtonsPrevious = buttons;

            Vector3 moveVector = data.movementInput.normalized;
            networkCharacterController.Move(moveSpeed * moveVector * Runner.DeltaTime); //base on tick not frame >>Runner
            
            if (pressed.IsSet(InputButtons.JUMP))
            {
                networkCharacterController.Jump();
            }

            if (pressed.IsSet(InputButtons.FIRE))
            {
                Runner.Spawn(
                    bulletPrefab,
                    transform.position + transform.TransformDirection(Vector3.forward),
                    Quaternion.LookRotation(transform.TransformDirection(Vector3.forward)),
                    Object.InputAuthority); //InputAuthority returns a ref which is the player controls
            }
        }
        if (Hp <= 0 || networkCharacterController.transform.position.y <= -5f) //Or out of boundry
        {
            Respawn();
        }
    }
    private void Respawn()
    {
        networkCharacterController.transform.position = Vector3.up * 2;
        Hp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (Object.HasStateAuthority)
        {
            Hp -= damage;
        }
    }
    private static void OnHpChanged(Changed<PlayerController> changed)
    {
        changed.Behaviour.hpBar.fillAmount = (float)changed.Behaviour.Hp / changed.Behaviour.maxHp;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeColor_RPC(Color.red);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ChangeColor_RPC(Color.green);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ChangeColor_RPC(Color.blue);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void ChangeColor_RPC(Color newColor)
    {
        meshRenderer.material.color = newColor;
    }
}
