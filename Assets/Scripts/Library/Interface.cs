using Script.Map;
using UnityEngine;

public interface IAnimationTrigger {
    void OnAnimationBegin();
    void OnAnimationEnd();
}

public interface IGUIController {
    void Pause(bool value);
}

public interface IMapObject {
    void Create(Race.Information information, Field field);
    Race.Information GetInformation();
    bool IsCreatable(Race.Information information, Field field);
    bool IsUnit();
}
