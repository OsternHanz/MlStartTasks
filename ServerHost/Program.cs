﻿using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace ServerHost
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                Console.WriteLine("Сервер запущен. Ожидаем соединения...");

                while (true)
                {
                    Socket handler = await sListener.AcceptAsync();
                    _ = Task.Run(async () =>
                    {
                        await HandleClient(handler, ipEndPoint);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        static async Task HandleClient(Socket handler, IPEndPoint ipEndPoint)
        {
            try
            {
                while (true)
                {
                    // Мы дождались клиента, пытающегося с нами соединиться
                    byte[] bytes = new byte[1024];
                    int bytesRec = await handler.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);

                    string data = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                    // Показываем данные на консоли
                    Console.WriteLine($"Получено от клиента: {data}");

                    
                    string reply = await ProcessData(data, ipEndPoint, handler);
                    byte[] msg = Encoding.UTF8.GetBytes(reply);
                    await handler.SendAsync(new ArraySegment<byte>(msg), SocketFlags.None);

                    if (data.Contains("<TheEnd>"))
                    {
                        Console.WriteLine("Клиент завершил соединение.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
        static async Task<string> ProcessData(string data, IPEndPoint ipEndPoint,Socket handler)
        {
            string[] parts = data.Split(' ');
            string command = parts[0];
            Console.WriteLine("Command: "+ command.Trim().ToUpper());
            switch (command.Trim().ToUpper())
            {
                case "LOG":
                    string login = parts[1];
                    string password = parts[2];
                    Console.WriteLine($"ConvertedData {parts[0]} {parts[1]} {parts[2]}");
                    if (!ClassForAuth.CheckHashAndLog(login, password))
                    {
                        return "Invalid credentials";
                    }
                    else
                    {
                        return "You have successfully logged in";
                    }
                case "REG":
                    string loginForReg = parts[1];
                    string passwordForReg = parts[2];
                    return ClassForAuth.RegIn(loginForReg, passwordForReg);
                case "CON":
                    return $"You are connected to IP: {ipEndPoint.Address}, Port: {ipEndPoint.Port}";
                case "LOR":
                    _ = SendLinesWithDelay(handler);
                    return "";
                case "CNF": //num1 + num2 + delay
                    try
                    {
                        MainFunProgram.num1 = int.Parse(parts[1]);
                        MainFunProgram.num2 = int.Parse(parts[2]);
                        MainFunProgram.delayInSeconds = int.Parse(parts[3]);
                        MainFunProgram.CoreMain();
                    }
                    catch(Exception ex)
                    {
                        return ex.Message.ToString();
                    }
                    //return $"Configuration Succsess {parts[1]}, {parts[2]}, {parts[3]}";
                    return "";

                default:
                    return "Incorrect Command";
            }
        }
        static async Task SendLinesWithDelay(Socket handler)
        {
            try
            {
                foreach (string line in MainFunProgram.lines)
                {
                    byte[] msg = Encoding.UTF8.GetBytes(line);
                    await handler.SendAsync(new ArraySegment<byte>(msg), SocketFlags.None);
                    await Task.Delay(TimeSpan.FromSeconds(MainFunProgram.delayInSeconds)); // Задержка перед отправкой следующего элемента
                }

                byte[] endMsg = Encoding.UTF8.GetBytes("<EndOfTransmission>");
                await handler.SendAsync(new ArraySegment<byte>(endMsg), SocketFlags.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}