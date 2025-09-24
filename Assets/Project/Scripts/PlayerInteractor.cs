using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private float rayDistance = 3.5f;
    [SerializeField] private LayerMask plotMask;
    [SerializeField] private Transform pickUpTarget;

    FarmTile _hover;

    void Update()
    {
        Ray r = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(r, out RaycastHit hit, rayDistance, plotMask))
        {
            var tile = hit.collider.GetComponentInParent<FarmTile>();
            if (_hover != tile)
            {
                if (_hover) _hover.SetHighlight(false);
                _hover = tile;
                if (_hover) _hover.SetHighlight(true);
            }
        }
        else
        {
            if (_hover) _hover.SetHighlight(false);
            _hover = null;
        }

        if (Input.GetKeyDown(KeyCode.E) && _hover)
        {
            switch (_hover.State)
            {
                case PlotState.Empty:
                    _hover.Interact();
                    break;

                case PlotState.Planted:
                    _hover.Interact();
                    break;

                case PlotState.Ready:
                    _hover.Harvest(pickUpTarget);
                    break;
            }
        }
    }
}
