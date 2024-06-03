using UnityEngine;

public static class AnimationDebugging
{
    public static string GetAnimationParameters(Animator animator)
    {
        var result = "Animation Parms \r\n";
        foreach (var parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                result += $"{parameter.name}: {animator.GetBool(parameter.name)}\n";
            }
            else if (parameter.type == AnimatorControllerParameterType.Float)
            {
                result += $"{parameter.name}: {animator.GetFloat(parameter.name)}\n";
            }
            else if (parameter.type == AnimatorControllerParameterType.Int)
            {
                result += $"{parameter.name}: {animator.GetInteger(parameter.name)}\n";
            }
        }
        return $"{result}";
    }
}
