using System;
using System.Collections.Generic;
using WestRentACar.Entidades;
using MySql.Data.MySqlClient;
namespace WestRentACar.Datos
{
    public class Conexion
    {
        // Asegúrate de que los datos de conexión coincidan con tu DBeaver
        private string _cadena = "Server=localhost;Database=westrentacardb;Uid=root;Pwd=;";
        public MySqlConnection ObtenerConexion() => new MySqlConnection(_cadena);
    }

    public class ClienteDAL
    {
        private Conexion _conexion = new Conexion();
        
        public void RegistrarCliente(Cliente c)
        {
            using MySqlConnection conn = _conexion.ObtenerConexion();
            string sql = "INSERT INTO cliente (rut, nombre, correo) VALUES (@rut, @nom, @cor)";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@rut", c.Rut);
            cmd.Parameters.AddWithValue("@nom", c.Nombre);
            cmd.Parameters.AddWithValue("@cor", c.Correo);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
        
        public Cliente BuscarCliente(string rut)
        {
            using MySqlConnection conn = _conexion.ObtenerConexion();
            string sql = "SELECT * FROM cliente WHERE rut = @rut";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@rut", rut);
            conn.Open();
            using MySqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new Cliente
                {
                    IdCliente = Convert.ToInt32(dr["idCliente"]),
                    Rut = dr["rut"].ToString()!,
                    Nombre = dr["nombre"].ToString()!,
                    Correo = dr["correo"].ToString()!
                };
            }
            return null!;
        }
    }

    public class VehiculoDAL
    {
        private Conexion _conexion = new Conexion();
        
        public List<Vehiculo> ListarDisponibles()
        {
            List<Vehiculo> lista = new List<Vehiculo>();
            using MySqlConnection conn = _conexion.ObtenerConexion();
            string sql = "SELECT * FROM vehiculo WHERE idEstadoVehiculo = 1";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            conn.Open();
            using MySqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Vehiculo
                {
                    IdVehiculo = Convert.ToInt32(dr["idVehiculo"]),
                    Patente = dr["patente"].ToString()!,
                    Marca = dr["marca"].ToString()!,
                    Modelo = dr["modelo"].ToString()!,
                    Anio = Convert.ToInt32(dr["anio"]),
                    PrecioDiario = Convert.ToDecimal(dr["precioDiario"]),
                    IdEstadoVehiculo = Convert.ToInt32(dr["idEstadoVehiculo"]),
                    IdFlota = Convert.ToInt32(dr["idFlota"])
                });
            }
            return lista;
        }
    }

    public class OperacionDAL
    {
        private Conexion _conexion = new Conexion();

        public int CrearReserva(Reserva r)
        {
            using MySqlConnection conn = _conexion.ObtenerConexion();
            conn.Open();
            using MySqlTransaction tx = conn.BeginTransaction();
            try
            {
                string q1 = @"INSERT INTO reserva (fechaInicio, fechaTermino, costoTotal, idCliente, idVehiculo, idEstadoReserva) 
                              VALUES (@ini, @fin, @total, @cli, @veh, 2);
                              SELECT LAST_INSERT_ID();";
                MySqlCommand cmd1 = new MySqlCommand(q1, conn, tx);
                cmd1.Parameters.AddWithValue("@ini", r.FechaInicio);
                cmd1.Parameters.AddWithValue("@fin", r.FechaTermino);
                cmd1.Parameters.AddWithValue("@total", r.CostoTotal);
                cmd1.Parameters.AddWithValue("@cli", r.IdCliente);
                cmd1.Parameters.AddWithValue("@veh", r.IdVehiculo);
                int idGenerado = Convert.ToInt32(cmd1.ExecuteScalar());

                string q2 = "UPDATE vehiculo SET idEstadoVehiculo = 2 WHERE idVehiculo = @veh";
                MySqlCommand cmd2 = new MySqlCommand(q2, conn, tx);
                cmd2.Parameters.AddWithValue("@veh", r.IdVehiculo);
                cmd2.ExecuteNonQuery();

                tx.Commit();
                return idGenerado;
            }
            catch { tx.Rollback(); throw; }
        }

        public void RegistrarDevolucion(Devolucion d)
        {
            using MySqlConnection conn = _conexion.ObtenerConexion();
            conn.Open();
            using MySqlTransaction tx = conn.BeginTransaction();
            try
            {
                string q1 = "INSERT INTO devolucion (fechaDevolucion, observaciones, idReserva) VALUES (@fec, @obs, @res)";
                MySqlCommand cmd1 = new MySqlCommand(q1, conn, tx);
                cmd1.Parameters.AddWithValue("@fec", d.FechaDevolucion);
                cmd1.Parameters.AddWithValue("@obs", d.Observaciones);
                cmd1.Parameters.AddWithValue("@res", d.IdReserva);
                cmd1.ExecuteNonQuery();

                string qV = "SELECT idVehiculo FROM reserva WHERE idReserva = @res";
                MySqlCommand cmdV = new MySqlCommand(qV, conn, tx);
                cmdV.Parameters.AddWithValue("@res", d.IdReserva);
                int idVeh = Convert.ToInt32(cmdV.ExecuteScalar());

                string q2 = "UPDATE vehiculo SET idEstadoVehiculo = 1 WHERE idVehiculo = @veh";
                MySqlCommand cmd2 = new MySqlCommand(q2, conn, tx);
                cmd2.Parameters.AddWithValue("@veh", idVeh);
                cmd2.ExecuteNonQuery();

                string q3 = "UPDATE reserva SET idEstadoReserva = 3 WHERE idReserva = @res";
                MySqlCommand cmd3 = new MySqlCommand(q3, conn, tx);
                cmd3.Parameters.AddWithValue("@res", d.IdReserva);
                cmd3.ExecuteNonQuery();

                tx.Commit();
            }
            catch { tx.Rollback(); throw; }
        }

        public Comprobante ProcesarPago(DetallePago p)
        {
            using MySqlConnection conn = _conexion.ObtenerConexion();
            conn.Open();
            using MySqlTransaction tx = conn.BeginTransaction();
            try
            {
                string q1 = "INSERT INTO detallepago (fechaPago, total, idMetodoPago, idReserva) VALUES (@fec, @tot, @met, @res); SELECT LAST_INSERT_ID();";
                MySqlCommand cmd1 = new MySqlCommand(q1, conn, tx);
                cmd1.Parameters.AddWithValue("@fec", p.FechaPago);
                cmd1.Parameters.AddWithValue("@tot", p.Total);
                cmd1.Parameters.AddWithValue("@met", p.IdMetodoPago);
                cmd1.Parameters.AddWithValue("@res", p.IdReserva);
                int idPagoGenerado = Convert.ToInt32(cmd1.ExecuteScalar());

                string q2 = "INSERT INTO comprobante (fechaEmision, montoTotal, idPago) VALUES (@fec, @tot, @idPago); SELECT LAST_INSERT_ID();";
                MySqlCommand cmd2 = new MySqlCommand(q2, conn, tx);
                cmd2.Parameters.AddWithValue("@fec", p.FechaPago);
                cmd2.Parameters.AddWithValue("@tot", p.Total);
                cmd2.Parameters.AddWithValue("@idPago", idPagoGenerado);
                int idComp = Convert.ToInt32(cmd2.ExecuteScalar());

                tx.Commit();
                return new Comprobante { IdComprobante = idComp, FechaEmision = p.FechaPago, MontoTotal = p.Total, IdPago = idPagoGenerado };
            }
            catch { tx.Rollback(); throw; }
        }

        public List<Reserva> ConsultarHistorial(string rut)
        {
            List<Reserva> lista = new List<Reserva>();
            using MySqlConnection conn = _conexion.ObtenerConexion();
            string sql = @"SELECT r.* FROM reserva r 
                           INNER JOIN cliente c ON r.idCliente = c.idCliente 
                           WHERE c.rut = @rut";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@rut", rut);
            conn.Open();
            using MySqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Reserva
                {
                    IdReserva = Convert.ToInt32(dr["idReserva"]),
                    IdVehiculo = Convert.ToInt32(dr["idVehiculo"]),
                    FechaInicio = Convert.ToDateTime(dr["fechaInicio"]),
                    FechaTermino = Convert.ToDateTime(dr["fechaTermino"]),
                    CostoTotal = Convert.ToDecimal(dr["costoTotal"]),
                    IdEstadoReserva = Convert.ToInt32(dr["idEstadoReserva"])
                });
            }
            return lista;
        }
    }
}