using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player: MonoBehaviour
{
    //movimiento basico
    //[SerializeField] CharacterController controller;
    public Rigidbody rb;
    [SerializeField] float speed;
    [SerializeField] float turnSmoothTime;
    [SerializeField] Transform cam;
    [SerializeField] float turnSmoothVelocity;
    public bool canMove;
    //sprint
    public bool isSprinting;
    [SerializeField] float sprintMultiplier;
    private float sprintSpeed = 1;


    //coordenadas para animaciones de movimiento
    [SerializeField] float x, y;
    public Animator anim;

    //ataque
    [SerializeField] int numeroArma;
    public bool atacando=false;
    public bool atacandoDebil = false;
    public bool atacandoFuerte = false;
    public int numero_golpesDebiles=0;
    public int numero_golpesFuertes=0;
    [SerializeField] Collider ColliderArma;
    [SerializeField] Collider ColliderPierna;

    //esquive
    public bool isDashing;

    //timer para parry
    public float tiempoTranscurrido;  

    //Lock On system
    //private Transform currentTarget;
    //public Transform player;

    //instancias y herencias
    public static Player instance;
    //[SerializeField] LockOnSystem lockOn;
    [SerializeField] cambiarArma cambioArma;

    AudioManager audioManager;
    private void Start()
    {
        Cursor.visible = false;
        canMove = true;

        ColliderArma.enabled=false;
        ColliderPierna.enabled = false;
        tiempoTranscurrido = 0f;
        rb = GetComponent<Rigidbody>();

    }

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("audio").GetComponent<AudioManager>();
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

            golpeCheck();
            bloqueoCheck();
            runCheck();

            if (x == 0f && y == 0f)
            {
                audioManager.stopFootstep();
            }
            else
            {
                audioManager.playFootstep(isSprinting);

            }
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
       
        Vector3 movement = movDir.normalized * speed * Time.deltaTime * sprintSpeed;
        rb.MovePosition(transform.position + movement);
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
       numeroArma=cambioArma.getterArma();

        if (numeroArma == 1)
        {
            anim.SetBool("distance", false);
            
            if (Input.GetMouseButton(0) && !atacando && !isSprinting)
            {

                atacando = true;
                atacandoDebil = true;

            }
            if (Input.GetMouseButton(1) && !atacando && !isSprinting)
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
        else if (numeroArma == 2)
        {
            anim.SetBool("distance", true);

    


        }

       

    }
   

    //verifica si el jugador esta manteniendo oprimido o solo oprimiendo la tecla para bloquear
    public void bloqueoCheck()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
               
             anim.SetBool("blocking", true);
            tiempoTranscurrido += Time.deltaTime;
            


        }
        
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            
            GetComponent<Collider>().enabled = true;
            anim.SetBool("blocking", false);
            
        }




    }
    public void ResetTimer()
    {
        tiempoTranscurrido = 0f;
    }
    public void bloquearDespuesDeGolpe()
    {
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }


    // metodos para el sistema de enfocarse en un enemigo

    //public void SetTarget(Transform target)
    //{
    //    currentTarget = target;
    //}

    //public void ClearTarget()
    //{
    //    currentTarget = null;
    //}


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
