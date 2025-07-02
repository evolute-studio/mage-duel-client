using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace TerritoryWars.Tools
{
    public sealed class Coroutines : MonoBehaviour
    {
        private static Coroutines _instance
        {
            get
            {
                if (m_instance == null)
                {
                    var go = new GameObject("[COROUTINE MANAGER]");
                    m_instance = go.AddComponent<Coroutines>();
                    DontDestroyOnLoad(go);
                }

                return m_instance;
            }
        }

        private static Coroutines m_instance;

        public static Coroutine StartRoutine(IEnumerator routine)
        {
            return _instance.StartCoroutine(routine);
        }

        public static void StopRoutine(Coroutine routine)
        {
            if (routine == null)
            {
                return;
            }

            _instance.StopCoroutine(routine);
        }

        public static async Task CoroutineAsync(Action action, float delay = 0f)
        {
            var tcs = new TaskCompletionSource<bool>();
            StartRoutine(WaitForCoroutine(tcs, action, delay));
            await tcs.Task;
        }

        private static IEnumerator WaitForCoroutine(TaskCompletionSource<bool> tcs, Action action, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            action();
            tcs.TrySetResult(true);
        }

        // Coroutine as async method
        public static async Task StartRoutineAsync(IEnumerator routine)
        {
            var awaiter = new CoroutineAwaiter(routine);
            await awaiter.RunRoutine();
        }

        private class CoroutineAwaiter
        {
            private readonly IEnumerator m_routine;
            private Coroutine m_coroutine;
            private readonly TaskCompletionSource<bool> tsc;

            public CoroutineAwaiter(IEnumerator routine)
            {
                tsc = new TaskCompletionSource<bool>();
                m_routine = routine;
            }

            public async Task<bool> RunRoutine()
            {
                m_coroutine = StartRoutine(Run());
                return await tsc.Task;
            }

            private IEnumerator Run()
            {
                yield return m_routine;
                tsc.SetResult(true);
                m_coroutine = null;
            }
        }
    }
}