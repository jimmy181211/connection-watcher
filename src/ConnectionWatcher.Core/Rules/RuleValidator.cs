using System.Net;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Rules;

public enum RuleValidationError
{
    NameRequired,
    InvalidRemoteIp,
    InvalidRemotePort,
    InvalidLocalPort,
    AtLeastOneConditionRequired,
    LocalListenerPortRequired,
    InvalidRepeatInterval
}

public static class RuleValidator
{
    public static IReadOnlyList<RuleValidationError> Validate(MonitoringRule rule)
    {
        List<RuleValidationError> errors = [];

        if (string.IsNullOrWhiteSpace(rule.Name))
        {
            errors.Add(RuleValidationError.NameRequired);
        }

        if (!string.IsNullOrWhiteSpace(rule.RemoteIp) &&
            !IPAddress.TryParse(rule.RemoteIp, out _))
        {
            errors.Add(RuleValidationError.InvalidRemoteIp);
        }

        if (!rule.RemotePort.IsValid)
        {
            errors.Add(RuleValidationError.InvalidRemotePort);
        }

        if (!rule.LocalPort.IsValid)
        {
            errors.Add(RuleValidationError.InvalidLocalPort);
        }

        if (rule.Type == MonitoringRuleType.LocalListener)
        {
            if (rule.LocalPort.IsAny)
            {
                errors.Add(RuleValidationError.LocalListenerPortRequired);
            }
        }
        else if (string.IsNullOrWhiteSpace(rule.RemoteIp) &&
                 rule.RemotePort.IsAny && rule.LocalPort.IsAny)
        {
            errors.Add(RuleValidationError.AtLeastOneConditionRequired);
        }

        if (rule.Action == MatchAction.PopupAlert &&
            rule.RepeatAlertMinutes is not (0 or 1 or 5 or 15))
        {
            errors.Add(RuleValidationError.InvalidRepeatInterval);
        }

        return errors;
    }
}
