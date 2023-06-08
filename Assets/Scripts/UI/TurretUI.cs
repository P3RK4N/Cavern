using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.AI.Navigation;

public class TurretUI : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [SerializeField]
    GameObject f_TurretPlaceholder;
    [SerializeField]
    GameObject f_Turret;
    [SerializeField]
    Material f_Material;

    Camera r_CAM;
    Transform r_TurretPlaceholder;
    Transform r_Player;
    Transform r_Turrets;
    NavMeshSurface r_NMS;

    static Vector3 s_HalfWidths = Vector3.zero;
    static Color s_PlaceableColor;
    static Color s_UnplaceableColor;

    bool m_Placeable = false;
    Material m_Material = null;

    void Awake()
    {
        if(s_HalfWidths == Vector3.zero)
        {
            s_HalfWidths = new Vector3(0.9f, 1.0f, 0.9f);
            s_PlaceableColor = Converter.toColor(0x76E544CD);
            s_UnplaceableColor = Converter.toColor(0xFF0213CD);
            r_Turrets = new GameObject("Turrets").transform;
        }

        r_CAM = FindObjectOfType<Camera>();
        r_Player = FindObjectOfType<Player>().transform;
        f_TurretPlaceholder.SetActive(false);
        r_TurretPlaceholder = f_TurretPlaceholder.transform;
        r_NMS = FindObjectOfType<NavMeshSurface>();

        m_Material = Instantiate(f_Material);
    }

    void Start()
    {
        setupMaterial();
    }

    void setupMaterial()
    {
        for(int i = 0; i < r_TurretPlaceholder.childCount; i++)
        {
            r_TurretPlaceholder.GetChild(i).GetComponent<MeshRenderer>().material = m_Material;
        }
    }

    void Update()
    {
        if(f_TurretPlaceholder.activeSelf) checkSpace();
    }

    void checkSpace()
    {
        bool placeable = true;

        if(Vector3.SqrMagnitude(r_TurretPlaceholder.position - r_Player.position) > 10.0f) placeable = false;
        else
        {
            int turretMask = 
                Layer.s_Instance.m_TurretMask   |
                Layer.s_Instance.m_EnemyMask    |
                Layer.s_Instance.m_PlayerMask   |
                Layer.s_Instance.m_FoodMask     |
                Layer.s_Instance.m_NestMask     |
                Layer.s_Instance.m_ObstacleMask;

            placeable = !Physics.CheckBox(r_TurretPlaceholder.position, s_HalfWidths, Quaternion.identity, turretMask, QueryTriggerInteraction.Collide);
        }

        if(placeable != m_Placeable)
        {
            m_Placeable = placeable;
            m_Material.color = m_Placeable ? s_PlaceableColor : s_UnplaceableColor;
        }
    }

    public void OnBeginDrag(PointerEventData ped)
    {
        f_TurretPlaceholder.SetActive(true);
        m_Material.color = s_UnplaceableColor;
    }

    public void OnEndDrag(PointerEventData ped)
    {
        f_TurretPlaceholder.SetActive(false);
        if(m_Placeable)
        {
            Instantiate(f_Turret, r_TurretPlaceholder.position, Quaternion.identity, r_Turrets);
            m_Placeable = false;
        }
    }

    public void OnDrag(PointerEventData ped)
    {
        int rayMask = Layer.s_Instance.m_GroundMask;

        Vector3 normalizedMousePos = new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, 1.0f);
        Ray ray = r_CAM.ViewportPointToRay(normalizedMousePos);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 50.0f, rayMask))
        {
            r_TurretPlaceholder.position = roundToLowerOdd(hit.point);
        }
    }

    public Vector3 roundToLowerOdd(Vector3 v)
    {
        return new Vector3(roundToLowerOdd(v.x), 0.0f, roundToLowerOdd(v.z));
    }

    private float roundToLowerOdd(float value)
    {
        int intValue = Mathf.CeilToInt(value);
        int oddValue = intValue % 2 == 0 ? intValue - 1 : intValue;
        return oddValue;
    }
}
