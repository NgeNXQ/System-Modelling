package LibTest;

import java.util.ArrayList;

import PetriObj.ArcIn;
import PetriObj.ArcOut;
import PetriObj.PetriP;
import PetriObj.PetriT;
import PetriObj.PetriNet;
import PetriObj.PetriSim;
import PetriObj.PetriObjModel;
import PetriObj.ExceptionInvalidTimeDelay;
import PetriObj.ExceptionInvalidNetStructure;

public class Task3
{
    public static void main(String[] args) throws ExceptionInvalidTimeDelay, ExceptionInvalidNetStructure
    {
        PetriObjModel model = buildModel();
        model.setIsProtokol(false);
        model.go(1000);

        System.out.print("Отриманий прибуток: ");
        System.out.println(model.getListObj().get(2).getNet().getListP()[5].getMark());

        System.out.print("Втрачений прибуток: ");
        System.out.println(model.getListObj().get(0).getNet().getListP()[4].getMark());

        System.out.print("Середня довжина черги у місті А: ");
        System.out.println(model.getListObj().get(0).getNet().getListP()[3].getMean());

        System.out.print("Середня довжина черги у місті Б: ");
        System.out.println(model.getListObj().get(1).getNet().getListP()[3].getMean());
    }

    private static PetriObjModel buildModel() throws ExceptionInvalidTimeDelay, ExceptionInvalidNetStructure
    {
        ArrayList<PetriSim> list = new ArrayList<PetriSim>();

        list.add(new PetriSim(InstantiateQueue("МІСТО_А")));
        list.add(new PetriSim(InstantiateQueue("МІСТО_Б")));
        list.add(new PetriSim(InstantiateRoute("АВТОБУС_1_МІСТО_А", 20.0)));
        list.add(new PetriSim(InstantiateRoute("АВТОБУС_2_МІСТО_А", 30.0)));
        list.add(new PetriSim(InstantiateRoute("АВТОБУС_1_МІСТО_Б", 20.0)));
        list.add(new PetriSim(InstantiateRoute("АВТОБУС_2_МІСТО_Б", 30.0)));

        list.get(2).getNet().getListT()[0].setPriority(1);
        list.get(4).getNet().getListT()[0].setPriority(1);

        // Місце "Втрачений прибуток" міста А => місце "Втрачений прибуток" міста Б
        list.get(0).getNet().getListP()[4] = list.get(1).getNet().getListP()[4];

        // Місце "Черга" міста А => місце "Черга пасажирів" автобуса 1 міста А
        list.get(0).getNet().getListP()[3] = list.get(2).getNet().getListP()[0];
        // Місце "Прибуття до іншого міста" автобуса 1 міста А => місце "Прибуття до вихідного міста" автобуса 1 міста Б
        list.get(2).getNet().getListP()[4] = list.get(4).getNet().getListP()[3];
        // Місце "Черга" міста Б => місце "Черга пасажирів" автобуса 1 міста Б
        list.get(1).getNet().getListP()[3] = list.get(4).getNet().getListP()[0];
        // Місце "Прибуття до вихідного міста" автобуса 1 міста А => місце "Прибуття до іншого міста" автобуса 1 міста Б
        list.get(2).getNet().getListP()[3] = list.get(4).getNet().getListP()[4];

        // Місце "Прибуток" автобуса 1 міста Б => місце "Прибуток" автобуса 2 міста А
        list.get(4).getNet().getListP()[5] = list.get(2).getNet().getListP()[5];
        // Місце "Прибуток" автобуса 2 міста А => місце "Прибуток" автобуса 1 міста А
        list.get(3).getNet().getListP()[5] = list.get(2).getNet().getListP()[5];
        // Місце "Прибуток" автобуса 2 міста Б => місце "Прибуток" автобуса 1 міста А
        list.get(5).getNet().getListP()[5] = list.get(2).getNet().getListP()[5];

        // Місце "Черга" міста А => місце "Черга пасажирів" автобуса 2 міста А
        list.get(0).getNet().getListP()[3] = list.get(3).getNet().getListP()[0];
        // Місце "Прибуття до іншого міста" автобуса 2 міста А => місце "Прибуття до вихідного міста" автобуса 2 міста Б
        list.get(3).getNet().getListP()[4] = list.get(5).getNet().getListP()[3];
        // Місце "Черга" міста Б => місце "Черга пасажирів" автобуса 2 міста Б
        list.get(1).getNet().getListP()[3] = list.get(5).getNet().getListP()[0];
        // Місце "Прибуття до вихідного міста" автобуса 2 міста А => місце "Прибуття до іншого міста" автобуса 2 міста Б
        list.get(3).getNet().getListP()[3] = list.get(5).getNet().getListP()[4];

        return new PetriObjModel(list);
    }

    private static PetriNet InstantiateQueue(String identifier) throws ExceptionInvalidNetStructure, ExceptionInvalidTimeDelay
    {
        ArrayList<PetriP> d_P = new ArrayList<>();
        ArrayList<PetriT> d_T = new ArrayList<>();
        ArrayList<ArcIn> d_In = new ArrayList<>();
        ArrayList<ArcOut> d_Out = new ArrayList<>();

        d_P.add(new PetriP("Генератор", 1));
        d_P.add(new PetriP("Зупинка", 0));
        d_P.add(new PetriP("Кількість пасажирів", 0));
        d_P.add(new PetriP("Черга", 0));
        d_P.add(new PetriP("Упущений прибуток", 0));

        d_T.add(new PetriT("Надходження", 0.5));
        d_T.get(0).setDistribution("unif", d_T.get(0).getTimeServ());
        d_T.get(0).setParamDeviation(0.2);
        d_T.add(new PetriT("Накопичення", 0.0));
        d_T.add(new PetriT("Відмова", 0.0));
        d_T.get(2).setPriority(1);

        d_In.add(new ArcIn(d_P.get(0), d_T.get(0), 1));
        d_In.add(new ArcIn(d_P.get(1), d_T.get(1), 1));
        d_In.add(new ArcIn(d_P.get(1), d_T.get(2), 1));
        d_In.add(new ArcIn(d_P.get(3), d_T.get(2), 30));
        d_In.get(3).setInf(true);
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(0), 1));
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(1), 1));
        d_Out.add(new ArcOut(d_T.get(1), d_P.get(3), 1));
        d_Out.add(new ArcOut(d_T.get(2), d_P.get(4), 20));
        d_Out.add(new ArcOut(d_T.get(1), d_P.get(2), 1));

        ArcIn.initNext();
        PetriP.initNext();
        PetriT.initNext();
        ArcOut.initNext();

        return new PetriNet(identifier, d_P, d_T, d_In, d_Out);
    }

    private static PetriNet InstantiateRoute(String identifier, double delay) throws ExceptionInvalidNetStructure, ExceptionInvalidTimeDelay
    {
        ArrayList<PetriP> d_P = new ArrayList<>();
        ArrayList<PetriT> d_T = new ArrayList<>();
        ArrayList<ArcIn> d_In = new ArrayList<>();
        ArrayList<ArcOut> d_Out = new ArrayList<>();

        d_P.add(new PetriP("Черга пасажирів", 0));
        d_P.add(new PetriP("Повний автобус", 0));
        d_P.add(new PetriP("Пустий автобус", 1));
        d_P.add(new PetriP("Прибуття до вихідного міста", 0));
        d_P.add(new PetriP("Прибуття до іншого міста", 0));
        d_P.add(new PetriP("Прибуток", 0));

        d_T.add(new PetriT("Посадка", 0.0));
        d_T.add(new PetriT("Переїзд", delay));
        d_T.get(1).setDistribution("unif", d_T.get(1).getTimeServ());
        d_T.get(1).setParamDeviation(5.0);
        d_T.add(new PetriT("Висадка", 5.0));
        d_T.get(2).setDistribution("unif", d_T.get(2).getTimeServ());
        d_T.get(2).setParamDeviation(1.0);

        d_In.add(new ArcIn(d_P.get(0), d_T.get(0), 1)); // people
        d_In.add(new ArcIn(d_P.get(1), d_T.get(1), 1));
        d_In.add(new ArcIn(d_P.get(2), d_T.get(0), 1));
        d_In.add(new ArcIn(d_P.get(3), d_T.get(2), 1));
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(1), 1));
        d_Out.add(new ArcOut(d_T.get(1), d_P.get(4), 1));
        d_Out.add(new ArcOut(d_T.get(1), d_P.get(5), 500));
        d_Out.add(new ArcOut(d_T.get(2), d_P.get(2), 1));

        ArcIn.initNext();
        PetriP.initNext();
        PetriT.initNext();
        ArcOut.initNext();

        return new PetriNet(identifier, d_P, d_T, d_In, d_Out);
    }
}
