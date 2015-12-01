using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopicQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            NamespaceManager manager = NamespaceManager.Create(); // Automatycznie bierze informacje z App.config
            //Wolę na początku - wygodniej "zaczynamy" zawsze od zera
            manager.DeleteTopic("obliczenia"); //Kasuje temat i subskrypcje
            manager.DeleteQueue("wynik");
            
            //Tworzenie Topics - tematu
            TopicDescription td = new TopicDescription("obliczenia");

            //Nie przewidujemy dużego ruchu nie wymagamy partycjonowania
            td.EnablePartitioning = false; 
            //Wymagamy wykrywania duplikatów - by klient 2 razy nie wysłał tego samego polecenia
            td.RequiresDuplicateDetection = true; 
            //Nie pozwalamy na tematy tylko w pamięci; chcemy żeby klient był pewien że wysłał wiadomość = wiadomość zostanie przetworzona
            td.EnableExpress = false; 
            manager.CreateTopic(td); //Tworzenie tematu

            //Suma i średnia będzie wyliczana gdy opowiednia własciwość zostanie zdefiniowana
            manager.CreateSubscription("obliczenia", "suma", new SqlFilter("suma=1"));
            manager.CreateSubscription("obliczenia", "srednia", new SqlFilter("srednia=1"));
            //Ale zawsze będą liczone elementy w komunikacie
            manager.CreateSubscription("obliczenia", "liczba");


            QueueDescription qd = new QueueDescription("wynik");
            qd.RequiresSession = true;
            manager.CreateQueue(qd);


        }
    }
}
