namespace TurnosMedicos.Helpers;

public static class DateTimeExtensions
{
    public static bool IsPasado(this DateTime fechaTurno)
    {
        return fechaTurno < DateTime.UtcNow;
    }

    public static bool EsCancelacionTardia(this DateTime fechaTurno)
    {
        return (fechaTurno - DateTime.UtcNow).TotalHours < 24;
    }
}