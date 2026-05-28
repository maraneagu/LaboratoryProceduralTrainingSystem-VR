using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ProcedureTimer
{
    [Serializable]
    public class Action
    {
        public bool isCorrect;

        public string expectedAction;
        public string performedAction;

        public float actionDuration;
        public float procedureDuration;
    }

    [Serializable]
    public class Procedure
    {
        public string participantId;
        public string participantDominantHand;
        public string timestamp;

        public string procedureName;
        public bool guidanceEnabled;
        public bool validationEnabled;

        public float procedureDuration;

        public int correctActionCount;
        public int incorrectActionCount;

        public List<Action> actions = new List<Action>();
    }

    private readonly string procedureName;

    private float procedureStartTime;
    private float lastActionStartTime;

    private bool procedureTimerStarted;
    public bool ProcedureTimerStarted => procedureTimerStarted;

    private readonly List<Action> actions = new List<Action>();

    public ProcedureTimer(string procedureName)
    {
        this.procedureName = procedureName;
    }

    public void StartProcedureTimer()
    {
        if (procedureTimerStarted)
            return;

        procedureTimerStarted = true;
        procedureStartTime = Time.time;
        lastActionStartTime = procedureStartTime;

        Debug.Log($"[ProcedureTimer] {procedureName} Procedure Timer Started!");
    }

    public void RecordAction(string expectedAction, string performedAction, bool isCorrect)
    {
        if (!procedureTimerStarted)
            StartProcedureTimer();

        float currentTime = Time.time;
        float actionDuration = currentTime - lastActionStartTime;
        float procedureDuration = currentTime - procedureStartTime;

        Action action = new Action
        {
            isCorrect = isCorrect,
            expectedAction = expectedAction,
            performedAction = performedAction,
            actionDuration = (float)Math.Round(actionDuration, 3),
            procedureDuration = (float)Math.Round(procedureDuration, 3)
        };

        actions.Add(action);
        lastActionStartTime = currentTime;

        Debug.Log(
            $"[ProcedureTimer] Action Recorded | " +
            $"{(isCorrect ? "Correct" : "Incorrect")} | " +
            $"Expected: {expectedAction} | " +
            $"Performed: {performedAction} | " +
            $"Action Duration: {actionDuration:F3}s | " +
            $"Procedure Duration: {procedureDuration:F3}s");
    }

    public float GetCurrentProcedureDuration()
    {
        if (!procedureTimerStarted)
            return 0f;

        return Time.time - procedureStartTime;
    }

    public void CompleteProcedure(
        string participantId,
        string participantDominantHand,
        bool guidanceEnabled,
        bool validationEnabled,
        int correctActionCount,
        int incorrectActionCount)
    {
        if (!procedureTimerStarted)
            return;

        float procedureDuration = Time.time - procedureStartTime;

        Procedure procedure = new Procedure
        {
            participantId = participantId,
            participantDominantHand = participantDominantHand,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            procedureName = procedureName,
            guidanceEnabled = guidanceEnabled,
            validationEnabled = validationEnabled,
            procedureDuration = (float)Math.Round(procedureDuration, 3),
            correctActionCount = correctActionCount,
            incorrectActionCount = incorrectActionCount,
            actions = new List<Action>(actions)
        };

        SaveProcedureToJson(procedure);

        Debug.Log($"[ProcedureTimer] {procedureName} Procedure Completed In {procedureDuration:F2} Seconds!");
    }

    private void SaveProcedureToJson(Procedure procedure)
    {
        string folderPath = Path.Combine(UnityEngine.Application.persistentDataPath, "Logs");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string safeParticipantId = string.IsNullOrWhiteSpace(procedure.participantId)
            ? "UnknownParticipant"
            : procedure.participantId.Replace(" ", "_");

        string safeProcedureName = string.IsNullOrWhiteSpace(procedure.procedureName)
            ? "Procedure"
            : procedure.procedureName.Replace(" ", "_");

        string fileName = $"{safeParticipantId}_{safeProcedureName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        string path = Path.Combine(folderPath, fileName);

        string json = JsonUtility.ToJson(procedure, true);
        File.WriteAllText(path, json);

        Debug.Log($"[ProcedureTimer] Procedure Timer Log Saved To: {path}!");
    }

    public void ResetProcedureTimer()
    {
        procedureTimerStarted = false;

        procedureStartTime = 0f;
        lastActionStartTime = 0f;

        actions.Clear();
    }
}