using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CharacterCustomisation : MonoBehaviour
{
    [SerializeField]
    private SpriteLibrary spriteLibrary = default;

    public SpriteResolver HairResolver = default;
    public SpriteResolver EyesResolver = default;
    public SpriteResolver ArmLeftResolver = default;
    public SpriteResolver ArmRightResolver = default;
    public SpriteResolver TopResolver = default;

    public string Hair;
    public string Eyes;
    public string Top;
    public string ArmL;
    public string ArmR;

    public TextMeshProUGUI HairtitleLabel;
    public TMP_Dropdown Hairdropdown;

    public TextMeshProUGUI EyestitleLabel;
    public TMP_Dropdown Eyesdropdown;

    public TMP_Dropdown Topsdropdown;

    public Toggle MaskToggle;
    public Text MaskLable;
    public Toggle ShortsToggle;
    public Text ShortsLable;
    public Toggle KnickersToggle;
    public Text KnickersLable;

    public GameObject HT;
    public GameObject HairObj;
    public GameObject Mask;
    public GameObject Knicker;
    public GameObject[] Shorts;

    public int HairIndex;
    public int EyesIndex;
    public int TopsIndex;
    private SpriteLibraryAsset LibraryAsset => spriteLibrary.spriteLibraryAsset;
    // Start is called before the first frame update
    void Start()
    {
        HairCustomise();
        EyesCustomise();
        TopCustomsie();
        MaskToggle.onValueChanged.AddListener(delegate { MaskValue(); });
        ShortsToggle.onValueChanged.AddListener(delegate { ShortsValue(); });
        KnickersToggle.onValueChanged.AddListener(delegate { KnickersValue(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HairCustomise()
    {
        // Set title
        HairtitleLabel.text = Hair;
        string[] labels = LibraryAsset.GetCategoryLabelNames(Hair).ToArray();

        // Populate dropdown
        List<TMP_Dropdown.OptionData> spriteLabels = new List<TMP_Dropdown.OptionData>();
        foreach (string label in labels)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(label);
            spriteLabels.Add(data);
        }

        Hairdropdown.options = spriteLabels;

        // Handle change
        Hairdropdown.onValueChanged.AddListener(optionIndex =>
        {
            string label = labels[optionIndex]; HairIndex = optionIndex;
            HairResolver.SetCategoryAndLabel(Hair, label);
        });
    }

    public void EyesCustomise()
    {
        // Set title
        EyestitleLabel.text = Eyes;
        string[] labels = LibraryAsset.GetCategoryLabelNames(Eyes).ToArray();

        // Populate dropdown
        List<TMP_Dropdown.OptionData> spriteLabels = new List<TMP_Dropdown.OptionData>();
        foreach (string label in labels)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(label);
            spriteLabels.Add(data);
        }

        Eyesdropdown.options = spriteLabels;

        // Handle change
        Eyesdropdown.onValueChanged.AddListener(optionIndex =>
        {
            string label = labels[optionIndex]; EyesIndex = optionIndex;
            EyesResolver.SetCategoryAndLabel(Eyes, label);
        });
    }

    public void TopCustomsie()
    {
        string[] Toplabels = LibraryAsset.GetCategoryLabelNames(Top).ToArray();
        string[] RAlabels = LibraryAsset.GetCategoryLabelNames(ArmR).ToArray();
        string[] LAlabels = LibraryAsset.GetCategoryLabelNames(ArmL).ToArray();

        // Handle change
        Topsdropdown.onValueChanged.AddListener(optionIndex =>
        {
            TopsIndex = optionIndex;
            string Toplabel = Toplabels[optionIndex];
            string RAlabel = RAlabels[optionIndex];
            string LAlabel = LAlabels[optionIndex];
            TopResolver.SetCategoryAndLabel(Top, Toplabel);
            ArmLeftResolver.SetCategoryAndLabel(ArmL, LAlabel);
            ArmRightResolver.SetCategoryAndLabel(ArmR, RAlabel);
        });
    }

    public void MaskValue()
    {
        if(MaskToggle.isOn)
        {
            Mask.gameObject.SetActive(true);
            HairObj.SetActive(false);
            HT.SetActive(false);
            MaskLable.text = "Mask On";
        }
        else
        {
            Mask.gameObject.SetActive(false);
            HairObj.SetActive(true);
            HT.SetActive(true);
            MaskLable.text = "Mask Off";
        }
    }

    public void ShortsValue()
    {
        if (ShortsToggle.isOn)
        {
            foreach (GameObject item in Shorts)
            {
                item.SetActive(true);
            }
            
            ShortsLable.text = "Shorts On";
        }
        else
        {
            foreach (GameObject item in Shorts)
            {
                item.SetActive(false);
            }
            ShortsLable.text = "Shorts Off";
        }
    }

    public void KnickersValue()
    {
        if (KnickersToggle.isOn)
        {
            Knicker.gameObject.SetActive(true);
            KnickersLable.text = "Knickers On";
        }
        else
        {
            Knicker.gameObject.SetActive(false);
            KnickersLable.text = "Knickers Off";
        }
    }
}
