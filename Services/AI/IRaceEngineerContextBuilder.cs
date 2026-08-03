using QRO.Models;

namespace QRO.Services.AI
{
    public interface IRaceEngineerContextBuilder
    {
        /// <summary>
        /// Builds the system prompt for the race engineer, combining static role instructions with
        /// live session context (track, weather, car, driver rating) from the most recent telemetry snapshot.
        /// </summary>
        /// <param name="session">The latest session snapshot, or null if no telemetry has been received yet.</param>
        string BuildSystemPrompt(SessionModel session);
    }
}
