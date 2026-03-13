using UnityEngine;

public class Player3DScript : MonoBehaviour
{
    private CharacterController m_charCont;

    float m_horizontal;
    float m_vertical;

    public float P_speed = 0.3f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //hämtar CharacterController-komponenten från spelobjektet
        m_charCont = GetComponent<CharacterController>();


    }

    // Update is called once per frame
    void Update()
    {
        m_horizontal = Input.GetAxis("Horizontal");
        m_vertical = Input.GetAxis("Vertical");

        //skapar en vektor som representerar spelarens rörelse baserat på input och hastighet
        Vector3 m_playerMovement = new Vector3(m_horizontal, 0f, m_vertical) * P_speed;

        m_charCont.Move(m_playerMovement);
    }
}
