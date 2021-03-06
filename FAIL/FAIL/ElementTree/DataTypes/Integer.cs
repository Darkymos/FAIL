using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.DataTypes;
internal class Integer : Object
{
    public Integer(int value, Token? token = null) : base(value, token)
    {
    }

    public override Type GetType() => new(nameof(Integer));

    public static explicit operator String(Integer integer) => new(integer.Value.ToString(), integer.Token);
}
