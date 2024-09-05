using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player: MonoBehaviour
{
    //movimiento basico
    [SerializeField] CharacterController controller;
    [SerializeField] float speed = 5;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] Transform cam;
    [SerializeField] float turnSmoothVelocity;
    public bool canMove;
    //sprint
    [SerializeField] bool isSprinting;
    [SerializeField] float sprintMultiplier = 1.08f;
    private float sprintSpeed = 1;


    //coordenadas para animaciones de movimiento
    [SerializeField] float x, y;
    public Animator anim;

    //ataque

    public bool atacando=false;
    public bool atacandoDebil = false;
    public bool atacandoFuerte = false;
    public int numero_golpesDebiles=0;
    public int numero_golpesFuertes=0;
    [SerializeField] Collider ColliderArma;
    [SerializeField] Collider ColliderPierna;

    //esquive
    public bool isDashing;

    //Lock On system
    private Transform currentTarget;
    public Transform player;

    //instancias y herencias
    public static Player instance;
    [SerializeField] HealthBar healthbar;
    [SerializeField] LockOnSystem lockOn;

    private void Start()
    {
        Cursor.visible = false;
        anim = GetComponent<Animator>();
        canMove = true;

        ColliderArma.enabled=false;
        ColliderPierna.enabled = false;
    }

    private void Awake()
    {
        instance = this;
    }


    void Update()
    {
        if (!canMove)
        {
            return; 
        }
        else
        {

            x = Input.GetAxis("Horizontal");
            y = Input.GetAxis("Vertical");

            Vector3 direction = new Vector3(x, 0f, y).normalized;

            //if (lockOn.locked)
            //{
            //    anim.SetBool("locked", true);

            //    //if (currentTarget != null)
            //    //{
            //    //    Vector3 direction2 = (currentTarget.position - player.position).normalized;
            //    //    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction2.x, 0, direction2.z));
            //    //    player.rotation = Quaternion.Slerp(player.rotation, lookRotation, Time.deltaTime * 5f);
            //    //}
            //}
            //else
            //{
            //    anim.SetBool("locked", false);
            //}


            golpeCheck();
            bloqueoCheck();
            runCheck();

            if (direction.magnitude >= 0.1f && !atacando && !anim.GetBool("blocking"))
            {


                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                Vector3 movDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;




                transform.rotation = Quaternion.Euler(0f, angle, 0f);


                move(movDir);

                anim.SetFloat("Velx", x);
                anim.SetFloat("Vely", y);

            }
        }

    }
    public void move(Vector3 movDir)
    {
        controller.Move(movDir.normalized * speed * Time.deltaTime * sprintSpeed);

    }

    //verifica si el usuario oprimió la tecla LeftShift, si lo hizó multiplica la velocidad base
    public void runCheck()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(x == 0f && y == 0f)
            {
                isDashing = true;
            }
            else
            {
                isSprinting = true;
            }
            

        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;

        }

        if (isSprinting == true && !atacando)
        {
            sprintSpeed = sprintMultiplier;
            anim.SetBool("running", true);
        }
        else if (isSprinting == false)
        {
            sprintSpeed = 1;
            anim.SetBool("running", false);
        }
    }
    //verifica si el usuario oprimio el click y activa la animacion de golpe
    public void golpeCheck()
    {

        if (Input.GetMouseButton(0) && !atacando) 
        {
           
          atacando = true;
          atacandoDebil = true;

        }
        if (Input.GetMouseButton(1) && !atacando)
        {

         atacando = true;
         atacandoFuerte = true;

        }
        if (!atacando)
        {
            numero_golpesDebiles = 0;
            numero_golpesFuertes = 0;
        }

    }
   

    //verifica si el jugador esta manteniendo oprimido o solo oprimiendo la tecla para bloquear
    public void bloqueoCheck()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
               
             anim.SetBool("blocking", true);
        }
        
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            
            GetComponent<Collider>().enabled = true;
            anim.SetBool("blocking", false);
            
        }




    }
    public void bloquearDespuesDeGolpe()
    {
        GetComponent<Collider>().enabled = true;
    }


    // metodos para el sistema de enfocarse en un enemigo

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }


    //activar colliders de diferentes armas
    public void activarColliderArma()
    {
        ColliderArma.enabled = true;
    }
    public void desactivarColliderArma()
    {
        ColliderArma.enabled = false;
    }
    public void activarColliderPierna()
    {
        ColliderPierna.enabled = true;
    }
    public void desactivarColliderPierna()
    {
        ColliderPierna.enabled = false;
    }
    //setters y getters
    public float returnSpeed()
    {
        return speed;
    }
    public void setSpeed(float speed1)
    {
        speed = speed1;
    }

    public void setMultiplier(float m1)
    {
        sprintMultiplier = m1;
    }
  
}
