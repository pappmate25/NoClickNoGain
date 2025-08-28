using System.Collections.Generic;

public interface IAnalyticsEngine
{
    void LogEvent(IGameEventDetails gameEventDetails);

    IEnumerable<IAnalyticsEvent> GetAllEvents();
}
