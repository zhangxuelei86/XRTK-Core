﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace XRTK.Utilities
{
    /// <summary>
    /// The purpose of this class is to provide a cached reference to the main camera. Calling Camera.main
    /// executes a FindByTag on the scene, which will get worse and worse with more tagged objects.
    /// </summary>
    public static class CameraCache
    {
        private static Camera cachedCamera;

        /// <summary>
        /// Returns a cached reference to the main camera and uses Camera.main if it hasn't been cached yet.
        /// </summary>
        public static Camera Main
        {
            get
            {
                var mainCamera = Camera.main;

                if (mainCamera == null)
                {
                    mainCamera = new GameObject("Main Camera", typeof(Camera)) { tag = "MainCamera" }.GetComponent<Camera>();
                }

                mainCamera = cachedCamera == null ? Refresh(mainCamera) : cachedCamera;

                if (mainCamera == null)
                {
                    // No camera in the scene tagged as main. Let's search the scene for a GameObject named Main Camera
                    var cameras = UnityEngine.Object.FindObjectsOfType<Camera>();

                    switch (cameras.Length)
                    {
                        case 0:
                            Debug.LogError("No Cameras found in the scene!");
                            break;
                        case 1:
                            mainCamera = Refresh(cameras[0]);
                            break;
                        default:
                            // Search for main camera by name.
                            foreach (var camera in cameras)
                            {
                                if (camera.name == "Main Camera")
                                {
                                    mainCamera = Refresh(camera);
                                    break;
                                }
                            }

                            // If we still didn't find it, just set the first camera.
                            if (mainCamera == null)
                            {
                                Debug.LogWarning($"No main camera found. The Mixed Reality Toolkit used {cameras[0].name} as your main.");
                                mainCamera = Refresh(cameras[0]);
                            }

                            break;
                    }
                }

                return mainCamera;
            }
        }

        /// <summary>
        /// Set the cached camera to a new reference and return it
        /// </summary>
        /// <param name="newMain">New main camera to cache</param>
        public static Camera Refresh(Camera newMain)
        {
            if (newMain == null)
            {
                Debug.LogWarning("A null camera was passed into CameraCache.Refresh()");
            }

            return cachedCamera = newMain;
        }
    }
}
