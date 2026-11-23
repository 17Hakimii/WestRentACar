using System;
using System.Collections.Generic;
using System.Linq;
using WestRentACar.Datos;
using WestRentACar.Entidades;

namespace WestRentACar.Negocio
{
    public class Sistema
    {
        private ClienteDAL _cliDal = new ClienteDAL();
        private VehiculoDAL _vehDal = new VehiculoDAL();
        private OperacionDAL _opDal = new OperacionDAL();

        public void RegistrarCliente(string rut, string nombre, string correo)
        {
            if (_cliDal.BuscarCliente(rut) != null) throw new Exception("El cliente ya está registrado.");
            _cliDal.RegistrarCliente(new Cliente { Rut = rut, Nombre = nombre, Correo = correo });
        }

        // Se mantiene BuscarPorRut por si tu Program.cs lo sigue llamando
        public Cliente BuscarPorRut(string rut) => BuscarCliente(rut);

        public Cliente BuscarCliente(string rut)
        {
            return _cliDal.BuscarCliente(rut) ?? throw new Exception("Cliente no encontrado.");
        }

        public void AgregarVehiculo(Vehiculo v)
        {
            // Método vacío temporal para que Program.cs no tire error si lo tienes en alguna opción
        }

        public List<Vehiculo> ListarVehiculosDisponibles()
        {
            return _vehDal.ListarDisponibles();
        }

        public (int IdReserva, decimal CostoTotal) CrearReserva(string rut, int idVehiculo, DateTime inicio, DateTime termino)
        {
            if (termino < inicio) throw new Exception("La fecha de término no puede ser menor a la de inicio.");
            Cliente cli = BuscarCliente(rut);

            Vehiculo veh = _vehDal.ListarDisponibles().FirstOrDefault(v => v.IdVehiculo == idVehiculo)
                           ?? throw new Exception("El vehículo no existe o no está disponible.");

            int dias = (termino - inicio).Days;
            if (dias == 0) dias = 1;
            decimal costoTotal = dias * veh.PrecioDiario;

            Reserva r = new Reserva
            {
                FechaInicio = inicio,
                FechaTermino = termino,
                CostoTotal = costoTotal,
                IdCliente = cli.IdCliente,
                IdVehiculo = idVehiculo
            };

            int idReserva = _opDal.CrearReserva(r);
            return (idReserva, costoTotal);
        }

        // Recibe los 3 argumentos que envía Program.cs y los empaqueta para Datos
        public void RegistrarDevolucion(int idReserva, string observaciones, bool tieneDanos)
        {
            Devolucion d = new Devolucion
            {
                IdReserva = idReserva,
                FechaDevolucion = DateTime.Now,
                Observaciones = observaciones + (tieneDanos ? " (CON DAÑOS)" : "")
            };
            _opDal.RegistrarDevolucion(d);
        }

        // Por si Program.cs envía 4 argumentos (incluyendo el ID del vehículo)
        public void RegistrarDevolucion(int idReserva, int idVehiculo, string observaciones, bool tieneDanos)
        {
            RegistrarDevolucion(idReserva, observaciones, tieneDanos);
        }

        public Comprobante ProcesarPago(int idReserva, int idMetodoPago, decimal total)
        {
            DetallePago p = new DetallePago
            {
                IdReserva = idReserva,
                IdMetodoPago = idMetodoPago,
                Total = total,
                FechaPago = DateTime.Now
            };
            return _opDal.ProcesarPago(p);
        }

        public List<Reserva> ConsultarHistorial(string rut)
        {
            return _opDal.ConsultarHistorial(rut);
        }
    }
}