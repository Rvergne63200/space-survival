using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public static class UnityEventRuntimeDebugger
{
    public static void LogRuntimeListeners(UnityEventBase unityEvent)
    {
        if (unityEvent == null)
        {
            Debug.LogWarning("[UnityEventDebugger] Event is null");
            return;
        }

        // UnityEventBase.m_Calls
        FieldInfo callsField = typeof(UnityEventBase)
            .GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic);

        if (callsField == null)
        {
            Debug.LogError("[UnityEventDebugger] Cannot find m_Calls");
            return;
        }

        object calls = callsField.GetValue(unityEvent);
        if (calls == null)
        {
            Debug.Log("[UnityEventDebugger] No calls object");
            return;
        }

        // InvokableCallList.m_RuntimeCalls
        FieldInfo runtimeCallsField = calls.GetType()
            .GetField("m_RuntimeCalls", BindingFlags.Instance | BindingFlags.NonPublic);

        if (runtimeCallsField == null)
        {
            Debug.LogError("[UnityEventDebugger] Cannot find m_RuntimeCalls");
            return;
        }

        var runtimeCalls = runtimeCallsField.GetValue(calls) as IList;

        if (runtimeCalls == null || runtimeCalls.Count == 0)
        {
            Debug.Log("[UnityEventDebugger] No RUNTIME listeners (AddListener)");
            return;
        }

        Debug.Log($"[UnityEventDebugger] Runtime listeners count: {runtimeCalls.Count}");

        foreach (var call in runtimeCalls)
        {
            Type callType = call.GetType();

            FieldInfo targetField = callType
                .GetField("m_Target", BindingFlags.Instance | BindingFlags.NonPublic);

            FieldInfo methodField = callType
                .GetField("m_MethodName", BindingFlags.Instance | BindingFlags.NonPublic);

            object target = targetField?.GetValue(call);
            object method = methodField?.GetValue(call);

            Debug.Log($"[RuntimeListener] Target: {target} | Method: {method}");
        }
    }
}
