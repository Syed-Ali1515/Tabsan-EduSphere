using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Modules;

/// <summary>
/// Represents a single functional module of the portal (e.g. Assignments, Quizzes, AI Chatbot).
/// Modules are seeded at startup. Their Key is used throughout the application as a stable
/// identifier for policy checks, feature flags, and API guards.
/// </summary>
public class Module : BaseEntity
{
    /// <summary>
    /// Stable string key used in code for policy/feature checks (e.g. "assignment", "quiz", "ai_chat").
    /// Never changed after seeding — referenced by name in policies and UI conditions.
    /// </summary>
    public string Key { get; private set; } = default!;

    /// <summary>Human-readable display name shown in the Super Admin modules screen.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// When true the module cannot be deactivated (e.g. Authentication, SIS, Departments).
    /// Enforced at both the API and service layers.
    /// </summary>
    public bool IsMandatory { get; private set; }

    private Module() { }

    public Module(string key, string name, bool isMandatory = false)
    {
        Key = key;
        Name = name;
        IsMandatory = isMandatory;
    }
}
