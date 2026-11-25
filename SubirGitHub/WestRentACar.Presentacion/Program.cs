using System;
using WestRentACar.Entidades;
using WestRentACar.Negocio;

Sistema sistema = new Sistema();
while (true)
{
    Console.Clear();
    Console.WriteLine("=== WEST RENT A CAR LOS ÁNGELES ===");
    Console.WriteLine("1. Registrar Cliente (RF1)");
    Console.WriteLine("2. Agregar Vehículo a Flota (RF2)");
    Console.WriteLine("3. Ver Vehículos Disponibles");
    Console.WriteLine("4. Crear Reserva (RF3 - RF4)");
    Console.WriteLine("5. Registrar Devolución (RF5)");
    Console.WriteLine("6. Procesar Pago y Comprobante (RF6)");
    Console.WriteLine("7. Consultar Historial Cliente (RF7)");
    Console.WriteLine("0. Salir");
    Console.Write("\nSeleccione una opción: ");

    try
    {
        string op = Console.ReadLine()!;
        if (op == "0") break;

        switch (op)
        {
            case "1":
                Console.Write("RUT: "); string rutCli = Console.ReadLine()!;
                Console.Write("Nombre: "); string nomCli = Console.ReadLine()!;
                Console.Write("Correo: "); string corCli = Console.ReadLine()!;
                sistema.RegistrarCliente(rutCli, nomCli, corCli);
                Console.WriteLine("Cliente registrado con éxito.");
                break;

            case "2":
                Vehiculo v = new Vehiculo();
                Console.Write("Patente: "); v.Patente = Console.ReadLine()!;
                Console.Write("Marca: "); v.Marca = Console.ReadLine()!;
                Console.Write("Modelo: "); v.Modelo = Console.ReadLine()!;
                Console.Write("Año: "); v.Anio = int.Parse(Console.ReadLine()!);
                Console.Write("Precio Diario: "); v.PrecioDiario = decimal.Parse(Console.ReadLine()!);
                sistema.AgregarVehiculo(v);
                Console.WriteLine("Vehículo agregado a la flota.");
                break;

            case "3":
                var disp = sistema.ListarVehiculosDisponibles();
                if (disp.Count == 0)
                {
                    Console.WriteLine("No hay vehículos disponibles en este momento. Agregue uno en la opción 2.");
                }
                else
                {
                    foreach (var item in disp) 
                        Console.WriteLine($"ID: {item.IdVehiculo} -> {item.ObtenerDetalle()}");
                }
                break;

            case "4":
                Console.Write("RUT Cliente: "); string rut = Console.ReadLine()!;
                Console.Write("ID Vehículo: "); int idV = int.Parse(Console.ReadLine()!);
                Console.Write("Fecha Inicio (dd-mm-aaaa): "); 
                DateTime ini = DateTime.ParseExact(Console.ReadLine()!, "dd-MM-yyyy", null);
                Console.Write("Fecha Término (dd-mm-aaaa): "); 
                DateTime fin = DateTime.ParseExact(Console.ReadLine()!, "dd-MM-yyyy", null);
                
                var resultado = sistema.CrearReserva(rut, idV, ini, fin);
                Console.WriteLine($"Reserva creada con éxito. ID Reserva: {resultado.IdReserva} - Costo Total calculado: ${resultado.CostoTotal:N0}");
                break;

            case "5":
                Console.Write("ID Reserva: "); int idRes = int.Parse(Console.ReadLine()!);
                Console.Write("ID Vehículo: "); int idVeh = int.Parse(Console.ReadLine()!);
                Console.Write("Observaciones: "); string obs = Console.ReadLine()!;
                Console.Write("¿Tiene daños severos? (s/n): "); bool danos = Console.ReadLine()!.ToLower() == "s";
                sistema.RegistrarDevolucion(idRes, idVeh, obs, danos);
                Console.WriteLine("Devolución registrada y estado actualizado.");
                break;

            case "6":
                Console.Write("ID Reserva a pagar: "); int idR = int.Parse(Console.ReadLine()!);
                Console.Write("Monto Total: "); decimal monto = decimal.Parse(Console.ReadLine()!);
                Console.Write("Método (1: Tarjeta, 2: Efectivo): "); int met = int.Parse(Console.ReadLine()!);
                
                Comprobante comp = sistema.ProcesarPago(idR, met, monto);
                Console.WriteLine("\n" + comp.ObtenerDetalle());
                break;

            case "7":
                Console.Write("RUT Cliente: ");
                var hist = sistema.ConsultarHistorial(Console.ReadLine()!);
                foreach (var r in hist) Console.WriteLine(r.ObtenerDetalle());
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[ERROR]: {ex.Message}");
        Console.ResetColor();
    }
    Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
    Console.ReadKey();
}