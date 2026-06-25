using System.Collections.Generic;

public class EnemyTemplate
{
    public string EnemyId { get; set; }
    public string DefaultName { get; set; }
    public CoreAttributes BaseAttributes { get; set; }
    public float Radius { get; set; } = 1.0f;

    public List<string> Abilities { get; set; } = new List<string>();
}