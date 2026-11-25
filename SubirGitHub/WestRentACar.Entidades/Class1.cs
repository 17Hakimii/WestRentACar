namespace WestRentACar.Entidades
{
    public abstract class EntidadBase
    {
        public abstract string ObtenerDetalle();
    }

    public class Cliente : EntidadBase
    {
        public int IdCliente { get; set; }
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }

        public override string ObtenerDetalle() => $"[{Rut}] {Nombre} - {Correo}";
    }

    public class Vehiculo : EntidadBase
    {
        public int IdVehiculo { get; set; }
        public string Patente { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Anio { get; set; }
        public decimal PrecioDiario { get; set; }
        public int IdEstadoVehiculo { get; set; }
        public int IdFlota { get; set; } = 1;

        public override string ObtenerDetalle() => $"{Patente} | {Marca} {Modelo} ({Anio}) - ${PrecioDiario:N0}/día";
    }

    public class Reserva : EntidadBase
    {
        public int IdReserva { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }
        public decimal CostoTotal { get; set; }
        public int IdCliente { get; set; }
        public int IdVehiculo { get; set; }
        public int IdEstadoReserva { get; set; }

        public override string ObtenerDetalle() => $"Reserva #{IdReserva} | Vehículo ID: {IdVehiculo} | Inicio: {FechaInicio:dd-MM-yyyy} | Término: {FechaTermino:dd-MM-yyyy} | Total: ${CostoTotal:N0}";
    }

    public class Devolucion
    {
        public int IdDevolucion { get; set; }
        public DateTime FechaDevolucion { get; set; }
        public string Observaciones { get; set; }
        public int IdReserva { get; set; }
    }

    public class DetallePago
    {
        public int IdPago { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Total { get; set; }
        public int IdMetodoPago { get; set; }
        public int IdReserva { get; set; }
    }

    public class Comprobante : EntidadBase
    {
        public int IdComprobante { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public int IdPago { get; set; }

        public override string ObtenerDetalle() => $"=== COMPROBANTE #{IdComprobante} ===\nFecha: {FechaEmision}\nMonto Pagado: ${MontoTotal:N0}";
    }
}