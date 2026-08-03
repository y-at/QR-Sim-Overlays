using QRO.Models;
using System.Linq;
using System.Text;

namespace QRO.Services.AI
{
    public class RaceEngineerContextBuilder : IRaceEngineerContextBuilder
    {
        #region Fields

        private const string RoleInstructions =
            "You are an expert iRacing race engineer helping a driver dial in their car setup. " +
            "Give specific, actionable setup changes (e.g. exact clicks, mm, or percentage adjustments) rather than vague advice. " +
            "When asked for a baseline setup, propose a complete starting point for the car/track/conditions below. " +
            "Calibrate how aggressive or conservative the baseline setup should be to the driver's iRating and safety rating: " +
            "lower-rated or less experienced drivers should get a more forgiving, stable baseline, while higher-rated drivers can be given a sharper, more edge-of-grip setup. " +
            "When the driver gives feedback about a handling issue, ask for missing details only if truly necessary, otherwise suggest the most likely setup change to address it.";

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds the system prompt for the race engineer, combining static role instructions with
        /// live session context (track, weather, car, driver rating) from the most recent telemetry snapshot.
        /// </summary>
        public string BuildSystemPrompt(SessionModel session)
        {
            var builder = new StringBuilder();
            builder.AppendLine(RoleInstructions);
            builder.AppendLine();
            builder.AppendLine(BuildContextBlock(session));
            return builder.ToString();
        }

        #endregion

        #region Private Methods

        private string BuildContextBlock(SessionModel session)
        {
            var playerCar = session?.Cars?.FirstOrDefault(c => c.IsPlayerCar);

            if (session == null || playerCar == null)
            {
                return "Live session data is not currently available (the driver may not be in an active iRacing session). " +
                       "Ask the driver directly for the car, track, and conditions if you need them.";
            }

            var builder = new StringBuilder();
            builder.AppendLine("Current session:");
            builder.AppendLine($"- Track: {NullIfEmpty(session.TrackName) ?? "unknown"}");
            builder.AppendLine($"- Car: {NullIfEmpty(playerCar.CarName) ?? "unknown"}{(string.IsNullOrEmpty(playerCar.CarClassName) ? "" : $" ({playerCar.CarClassName})")}");
            builder.AppendLine($"- Track temp: {session.TrackTemp:0.#}C, Air temp: {session.AirTemp:0.#}C");
            builder.AppendLine($"- Wind: {session.WindSpeed:0.#} km/h at {session.WindDirection:0}deg");
            builder.AppendLine($"- Sky: {DescribeSkyState(session.SkyState)}{(session.WeatherDeclaredWet ? ", session declared wet" : "")}");
            builder.AppendLine($"- Track wetness: {session.TrackWetness}%");

            if (playerCar.Drivers != null)
            {
                builder.AppendLine($"- Driver iRating: {(playerCar.Drivers.IRating > 0 ? playerCar.Drivers.IRating.ToString() : "unknown")}");
                builder.AppendLine($"- Driver safety rating: {NullIfEmpty(playerCar.Drivers.SafetyRating) ?? "unknown"}");
            }

            return builder.ToString();
        }

        private static string DescribeSkyState(int skyState)
        {
            switch (skyState)
            {
                case 0: return "clear";
                case 1: return "scattered clouds";
                case 2: return "broken clouds";
                case 3: return "overcast";
                default: return "unknown";
            }
        }

        private static string NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

        #endregion
    }
}
