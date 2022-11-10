// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace MemoryLeakInvestigations
{
    public static class ObjectTracker
    {
        private static readonly object Monitor = new object();
        private static readonly List<TrackerEntry> Objects = new List<TrackerEntry>();

        [Conditional("DEBUG")]
        public static void Track(object objectToTrack)
        {
            if (IsEnabled)
            {
                lock (Monitor)
                {
                    if (objectToTrack is DependencyObject element)
                    {
                        TrackDependencyObject(element, 0);
                    }
                    else
                    {
                        TrackObject(objectToTrack, 0);
                    }
                }
            }
        }

        private static void TrackObject(object objectToTrack, int level)
        {
            if (Objects.FirstOrDefault(o => o.Object.IsAlive && o.Object.Target == objectToTrack) == null)
            {
                Objects.Add(new TrackerEntry() { Object = new WeakReference(objectToTrack), Indentation = level });
            }
        }

        private static void TrackDependencyObject(DependencyObject element, int level)
        {
            TrackObject(element, level);

            var childrenCount = VisualTreeHelper.GetChildrenCount(element);

            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);

                TrackDependencyObject(child, level + 1);
            }
        }

        private static bool isEnabled = true;

        public static bool IsEnabled
        {
            get
            {
                return isEnabled;
            }

            set
            {
                isEnabled = value;
                if (!value)
                {
                    Objects.Clear();
                }
            }
        }

        private static List<TrackerEntry> GetAllLiveTrackedObjects()
        {
            lock (Monitor)
            {
                var alive = new List<TrackerEntry>();
                for (int i = Objects.Count - 1; i >= 0; i--)
                {
                    var o = Objects[i];
                    if (o.Object.IsAlive)
                    {
                        alive.Add(o);
                    }
                    else
                    {
                        Objects.RemoveAt(i);
                    }
                }
                return alive;
            }
        }

        [Conditional("DEBUG")]
        public static void GarbageCollect()
        {
            if (!IsEnabled)
            {
                return;
            }

            // Garbage Collect
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var liveObjects = ObjectTracker.GetAllLiveTrackedObjects();

            StringBuilder sbStatus = new StringBuilder();

            Debug.WriteLine("---------------------------------------------------------------------");
            if (!liveObjects.Any())
            {
                sbStatus.AppendLine("No Memory Leaks.");
            }
            else
            {
                sbStatus.AppendLine("***    Possible memory leaks in the objects below or their children.   ***");
                sbStatus.AppendLine("*** Clear memory again and see if any of the objects free from memory. ***");

                for (int i = liveObjects.Count - 1; i >= 0; i--)
                {
                    var obj = liveObjects[i];
                    if (obj.Object.IsAlive)
                    {
                        PrintIndented(sbStatus, obj.Object.Target, obj.Indentation);
                    }
                }
            }
            Debug.WriteLine(sbStatus.ToString());
            Debug.WriteLine("---------------------------------------------------------------------");
        }

        private static void PrintIndented(StringBuilder sb, object obj, int level)
        {
            for (int i = 0; i < level; i++)
            {
                sb.Append("    ");
            }

            if (obj is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{obj.GetType()}, Name = {fe.Name}");
            }
            else
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{obj.GetType()}");
            }
        }

        private sealed class TrackerEntry
        {
            public WeakReference Object { get; set; }

            public int Indentation { get; set; }
        }
    }
}
