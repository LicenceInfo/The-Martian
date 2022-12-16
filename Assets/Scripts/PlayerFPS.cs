using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFPS : MonoBehaviour
{
    //Stats max du joueur
    [SerializeField]
    public float maxOxygen = 100; // a voir avec la combinaison
    [SerializeField]
    public float maxFood = 100;
    [SerializeField]
    public float maxWater = 100;
    [SerializeField]
    public float maxHealth = 100;
    [SerializeField]
    public float maxStamina = 100;

    //Stats du joueur
    [SerializeField]
    public float oxygen;
    [SerializeField]
    public float food;
    [SerializeField]
    public float water;
    [SerializeField]
    public float health;
    [SerializeField]
    public float stamina;

    [SerializeField]
    public Image oxygenBarFill;
    [SerializeField]
    public Image foodBarFill;
    [SerializeField]
    public Image waterBarFill;
    [SerializeField]
    public Image healthBarFill;
    [SerializeField]
    public Image staminaBarFill;



    //Si true on peut courir, sinon on ne peut pas
    public bool canRun;

    public bool oxygenAround;

    //Camera
    public Camera playerCamera;

    //Composant qui permet de faire bouger le joueur
    CharacterController characterController;

    //Vitesse de marche
    public float walkingSpeed = 7.5f;

    //Vitesse de course
    public float runningSpeed = 15f;

    //Vitesse de saut
    public float jumpSpeed = 8f;

    //Gravité
    float gravity = 20f;

    //Déplacement
    Vector3 moveDirection;

    //Marche ou court ?
    private bool isRunning = false;

    //Rotation de la caméra
    float rotationX = 0;
    public float rotationSpeed = 2.0f;
    public float rotationXLimit = 45.0f;

    public void SetOxygen(float value)
    {
        oxygen = value;
    }

    public void OxygenAdd(float value)
    {
        oxygen += value;
        if (oxygen > maxOxygen)
        {
            oxygen = maxOxygen;
        }
    }

    public void OxygenSub(float value)
    {
        oxygen -= value;
        if (oxygen <= 0)
        {
           food =0;
        }
    }

    public void FoodAdd(float value)
    {
        food += value;
        if (food > maxFood)
        {
            food = maxFood;
        } 
    }

    public void FoodSub(float value)
    {
        food -= value;
        if (food <= 0)
        {
            food = 0;
        }
    }

    public void WaterAdd(float value)
    {
        water += value;
        if (water > maxWater)
        {
            water = maxWater;
        } 
    }

    public void WaterSub(float value)
    {
        water -= value;
        if (water <= 0)
        {
            water = 0;
        } 
    }

    public void HealthAdd(float value)
    {
        health += value;
        if (health > maxHealth)
        {
            health = maxHealth;
        } 
    }

    public void HealthSub(float value)
    {
        health -= value;
        if (health <= 0)
        {
            health = 0;
        } 
    }

    public void StaminaAdd(float value)
    {
        stamina += value;
        if (stamina > maxStamina)
        {
            stamina = maxStamina;
        }
    }

    public void StaminaSub(float value)
    {
        stamina -= value;
        if (stamina <= 0)
        {
            stamina = 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Cache le curseur de la souris
        Cursor.visible = false;
        characterController = GetComponent<CharacterController>();

        oxygen = maxOxygen;
        food = maxFood;
        water = maxWater;
        health = maxHealth;
        stamina = maxStamina;
    }

    void TakeDamage(float damage) 
    {
        health -= damage;
        UpdateHealthBarFill();
    }

    void UpdateHealthBarFill()
    {
        healthBarFill.fillAmount = health / maxHealth;
    }

    void UpdateDisplay()
    {
        oxygenBarFill.fillAmount = oxygen / maxOxygen;
        foodBarFill.fillAmount = food / maxFood;
        waterBarFill.fillAmount = water / maxWater;
        staminaBarFill.fillAmount = stamina / maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(15f);
        }

        UpdateDisplay();

        //Calcule les directions
        //forward = avant/arrière
        //right = droite/gauche
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        //Est-ce qu'on appuie sur un bouton de direction ?

        // Z = axe arrière/avant
        float speedZ = Input.GetAxis("Vertical");

        // X = axe gauche/droite
        float speedX = Input.GetAxis("Horizontal");

        // Y = axe haut/bas
        float speedY = moveDirection.y;

        //On perd de la nourriture, de l'eau et de l'oxygen
        FoodSub(0.00001f);
        WaterSub(0.00001f);
        if (oxygenAround)
        {
            SetOxygen(maxOxygen);
        }
        else
        {
            OxygenSub(0.00001f);
        }

        //Est-ce qu'on appuie sur le bouton pour courir (ici : Shift Gauche) ? Est-ce qu'il nous reste de la stamina ?
        if (Input.GetKey(KeyCode.LeftShift) && canRun && food > 10 && water > 10)
        {
            //En train de courir
            isRunning = true;
            //On en perd plus si on cour
            FoodSub(0.00006f);
            WaterSub(0.00006f);
            OxygenSub(0.00006f);
        }
        else
        {
            //En train de marcher
            isRunning = false;
        }

        // Est-ce que l'on court ?
        if (isRunning)
        {
            //test si il nous reste de la stamina
            StaminaSub(0.02f);
            if (stamina == 0)
            {
                canRun = false;
            }
            //Multiplie la vitesse par la vitesse de course
            speedX = speedX * runningSpeed;
            speedZ = speedZ * runningSpeed;
        }
        else
        {
            //recharge la stamina
            StaminaAdd(0.04f);
            if (stamina > 10)
            {
                canRun = true;
            }
            //Multiplie la vitesse par la vitesse de marche
            speedX = speedX * walkingSpeed;
            speedZ = speedZ * walkingSpeed;
        }

        //Calcul du mouvement
        //forward = axe arrière/avant
        //right = axe gauche/droite
        moveDirection = forward * speedZ + right * speedX;


        // Est-ce qu'on appuie sur le bouton de saut (ici : Espace)
        if (Input.GetButton("Jump") && characterController.isGrounded)
        {
            StaminaSub(5);
            moveDirection.y = jumpSpeed;
        }
        else
        {
        moveDirection.y = speedY;
        }


        //Si le joueur ne touche pas le sol
        if (!characterController.isGrounded)
        {
            //Applique la gravité * deltaTime
            //Time.deltaTime = Temps écoulé depuis la dernière frame
            moveDirection.y -= gravity * Time.deltaTime;
        }


        //Applique le mouvement
        characterController.Move(moveDirection * Time.deltaTime);



        //Rotation de la caméra

        //Input.GetAxis("Mouse Y") = mouvement de la souris haut/bas
        //On est en 3D donc applique ("Mouse Y") sur l'axe de rotation X 
        rotationX += -Input.GetAxis("Mouse Y") * rotationSpeed;

        //La rotation haut/bas de la caméra est comprise entre -45 et 45 
        //Mathf.Clamp permet de limiter une valeur
        //On limite rotationX, entre -rotationXLimit et rotationXLimit (-45 et 45)
        rotationX = Mathf.Clamp(rotationX, -rotationXLimit, rotationXLimit);


        //Applique la rotation haut/bas sur la caméra
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);


        //Input.GetAxis("Mouse X") = mouvement de la souris gauche/droite
        //Applique la rotation gauche/droite sur le Player
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * rotationSpeed, 0);

        if (water == 0 || food == 0 || oxygen == 0)
        {
            health = 0;
        }

        /*if (health == 0)
        {
            //T mort sale merde
        } */

    }
}
