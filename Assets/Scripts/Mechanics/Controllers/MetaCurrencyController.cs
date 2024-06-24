using TMPro;

public class MetaCurrencyController : BaseCurrencyController
{
    TextMeshProUGUI tmp;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        tmp = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        tmp.text = player.metaCurrency.current.ToString();
    }
}
