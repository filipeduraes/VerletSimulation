using System;
using UnityEngine;
using Sample.Input;
using UnityEngine.InputSystem;

namespace Sample.Visual
{
    public class SimulationController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Simulation simulation;

        private VisualDot selectedDot;
        private SimulationControls _controls;
        
        private void Awake()
        {
            _controls = new SimulationControls();
        }

        private void OnEnable()
        {
            _controls.Enable();
            _controls.Controls.Select.started += SelectDot;
            _controls.Controls.ToggleLock.started += ToggleLock;
            _controls.Controls.ToggleSimulation.started += ToggleSimulation;
        }

        private void OnDisable()
        {
            _controls.Disable();
            _controls.Controls.Select.started -= SelectDot;
            _controls.Controls.ToggleLock.started -= ToggleLock;
            _controls.Controls.ToggleSimulation.started -= ToggleSimulation;
        }

        private void Update()
        {
            if (_controls.Controls.Break.IsPressed())
            {
                BreakConnectionsInMouse();
            }
        }

        private void OnDestroy()
        {
            _controls.Dispose();
        }
        
        private void ToggleSimulation(InputAction.CallbackContext context)
        {
            simulation.ToggleSimulationRunning();
            DeselectCurrentDot();
        }
        
        private void SelectDot(InputAction.CallbackContext context)
        {
            if (simulation.IsRunning())
            {
                return;
            }
            
            if (GetDotInMousePosition(out VisualDot visualDot))
            {
                if (visualDot == selectedDot)
                {
                    DeselectCurrentDot();
                }
                else if(selectedDot != null)
                {
                    simulation.CreateConnection(visualDot, selectedDot);
                    DeselectCurrentDot();
                }
                else
                {
                    SelectDot(visualDot);
                }
            }
            else if (selectedDot != null)
            {
                VisualDot createdDot = simulation.CreateAndConnectDot(GetWorldMousePosition(), selectedDot);
                DeselectCurrentDot();
                SelectDot(createdDot);
            }
        }
        
        private void ToggleLock(InputAction.CallbackContext context)
        {
            if (selectedDot != null)
            {
                selectedDot.RequestToggleLock();
            }
        }

        private bool GetDotInMousePosition(out VisualDot visualDot)
        {
            Vector3 worldMousePosition = GetWorldMousePosition();
            RaycastHit2D hit = Physics2D.Raycast(worldMousePosition, Vector2.zero);
            visualDot = null;

            return hit && hit.transform.gameObject.TryGetComponent(out visualDot);
        }
        
        private void BreakConnectionsInMouse()
        {
            Vector3 worldMousePosition = GetWorldMousePosition();
            simulation.BreakConnectionOnPosition(worldMousePosition);
        }

        private Vector3 GetWorldMousePosition()
        {
            Vector2 mousePosition = _controls.Controls.CursorPosition.ReadValue<Vector2>();
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            worldMousePosition.z = 0.0f;
            return worldMousePosition;
        }

        private void SelectDot(VisualDot dot)
        {
            dot.SetIsSelected(true);
            selectedDot = dot;
        }

        private void DeselectCurrentDot()
        {
            if (selectedDot != null)
            {
                selectedDot.SetIsSelected(false);
                selectedDot = null;
            }
        }
    }
}