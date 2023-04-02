using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DC.Scanner;

namespace DC.Test
{

    public class TestPlayer : MonoBehaviour
    {
        public TargetScanner scanner;

        Transform pos;

        // Update is called once per frame
        void FixedUpdate()
        {
            pos = scanner.GetNearestTarget();

        }

        private void OnDrawGizmos()
        {
            scanner.ShowGizmos();
        }
    }

}
