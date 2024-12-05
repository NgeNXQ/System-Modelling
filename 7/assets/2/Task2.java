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

public class Task2
{
    public static void main(String[] args) throws ExceptionInvalidTimeDelay, ExceptionInvalidNetStructure
    {
        PetriObjModel model = buildModel();
        model.setIsProtokol(false);
        model.go(10_000);

        System.out.print("Кількість створених деталей: ");
        System.out.println(model.getListObj().get(0).getNet().getListP()[2].getMark());

        System.out.print("Оброблено верстатом № 1: ");
        System.out.println(model.getListObj().get(4).getNet().getListP()[3].getMark());

        System.out.print("Оброблено верстатом № 2: ");
        System.out.println(model.getListObj().get(5).getNet().getListP()[3].getMark());

        System.out.print("Кількість деталей на складі: ");
        System.out.println(model.getListObj().get(3).getNet().getListP()[3].getMark());
    }

    private static PetriObjModel buildModel() throws ExceptionInvalidTimeDelay, ExceptionInvalidNetStructure
    {
        ArrayList<PetriSim> list = new ArrayList<>();

        list.add(new PetriSim(InstantiateGenerator("ГЕНЕРАТОР")));

        list.add(new PetriSim(InstantiateRobot("РОБОТ_1", 6)));
        list.add(new PetriSim(InstantiateRobot("РОБОТ_2", 7)));
        list.add(new PetriSim(InstantiateRobot("РОБОТ_3", 5)));
        list.add(new PetriSim(InstantiateWorkbench("ВЕРСТАК_1", "norm", 60, 10)));
        list.add(new PetriSim(InstantiateWorkbench("ВЕРСТАК_2", "exp", 100, 0)));

        // Місце "Пункт надходження" генератора => місце "Деталь надійшла" у робота 1
        list.get(0).getNet().getListP()[1] = list.get(1).getNet().getListP()[0];

        // Місце "Перенесена деталь" у робота 1 => місце "Деталь надійшла" у верстата 1
        list.get(1).getNet().getListP()[3] = list.get(4).getNet().getListP()[0];

        // Місце "Оброблена деталь" у верстата 1 => місце "Деталь надійшла" у робота 2
        list.get(4).getNet().getListP()[1] = list.get(2).getNet().getListP()[0];

        // Місце "Перенесена деталь" у робота 2 => місце "Деталь надійшла" у верстата 2
        list.get(2).getNet().getListP()[3] = list.get(5).getNet().getListP()[0];

        // Місце "Оброблена деталь" у верстата 2 => місце "Деталь надійшла" на складі у робота 3
        list.get(5).getNet().getListP()[1] = list.get(3).getNet().getListP()[0];

        return new PetriObjModel(list);
    }

    private static PetriNet InstantiateGenerator(String identifier) throws ExceptionInvalidNetStructure, ExceptionInvalidTimeDelay
    {
        ArrayList<PetriP> d_P = new ArrayList<>();
        ArrayList<PetriT> d_T = new ArrayList<>();
        ArrayList<ArcIn> d_In = new ArrayList<>();
        ArrayList<ArcOut> d_Out = new ArrayList<>();

        d_P.add(new PetriP("Генератор", 1));
        d_P.add(new PetriP("Пункт надходження", 0));
        d_P.add(new PetriP("Кількість надходжень", 0));

        d_T.add(new PetriT("Надходження", 40.0));
        d_T.get(0).setDistribution("exp", d_T.get(0).getTimeServ());
        d_T.get(0).setParamDeviation(0.0);

        d_In.add(new ArcIn(d_P.get(0), d_T.get(0), 1));
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(0), 1));
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(1), 1));
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(2), 1));

        ArcIn.initNext();
        PetriP.initNext();
        PetriT.initNext();
        ArcOut.initNext();

        return new PetriNet(identifier, d_P, d_T, d_In, d_Out);
    }

    private static PetriNet InstantiateRobot(String identifier, double movementDelay) throws ExceptionInvalidNetStructure, ExceptionInvalidTimeDelay
    {
        ArrayList<PetriP> d_P = new ArrayList<>();
        ArrayList<PetriT> d_T = new ArrayList<>();
        ArrayList<ArcIn> d_In = new ArrayList<>();
        ArrayList<ArcOut> d_Out = new ArrayList<>();

        d_P.add(new PetriP("Деталь надійшла", 0));
        d_P.add(new PetriP("Деталь захоплено", 0));
        d_P.add(new PetriP("Деталь перенесено", 0));
        d_P.add(new PetriP("Перенесена деталь", 0));
        d_P.add(new PetriP("Робот готовий", 1));
        d_P.add(new PetriP("Робот вільний", 0));

        d_T.add(new PetriT("Захоплення", 8.0));
        d_T.get(0).setDistribution("unif", d_T.get(0).getTimeServ());
        d_T.get(0).setParamDeviation(1.0);

        d_T.add(new PetriT("Перенесення", movementDelay));

        d_T.add(new PetriT("Звільнення", 8.0));
        d_T.get(2).setDistribution("unif", d_T.get(2).getTimeServ());
        d_T.get(2).setParamDeviation(1.0);

        d_T.add(new PetriT("Повернення", movementDelay));

        d_In.add(new ArcIn(d_P.get(0), d_T.get(0), 1));
        d_In.add(new ArcIn(d_P.get(1), d_T.get(1), 1));
        d_In.add(new ArcIn(d_P.get(2), d_T.get(2), 1));
        d_In.add(new ArcIn(d_P.get(5), d_T.get(3), 1));
        d_In.add(new ArcIn(d_P.get(4), d_T.get(0), 1));
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(1), 1));
        d_Out.add(new ArcOut(d_T.get(1), d_P.get(2), 1));
        d_Out.add(new ArcOut(d_T.get(2), d_P.get(3), 1));
        d_Out.add(new ArcOut(d_T.get(2), d_P.get(5), 1));
        d_Out.add(new ArcOut(d_T.get(3), d_P.get(4), 1));

        ArcIn.initNext();
        PetriP.initNext();
        PetriT.initNext();
        ArcOut.initNext();

        return new PetriNet(identifier, d_P, d_T, d_In, d_Out);
    }

    private static PetriNet InstantiateWorkbench(String identifier, String distributionType, double mean, double deviation) throws ExceptionInvalidNetStructure, ExceptionInvalidTimeDelay
    {
        ArrayList<PetriP> d_P = new ArrayList<>();
        ArrayList<PetriT> d_T = new ArrayList<>();
        ArrayList<ArcIn> d_In = new ArrayList<>();
        ArrayList<ArcOut> d_Out = new ArrayList<>();

        d_P.add(new PetriP("Деталь надійшла", 0));
        d_P.add(new PetriP("Оброблена деталь", 0));
        d_P.add(new PetriP("Ресурс верстату", 3));
        d_P.add(new PetriP("Кількість оброблених", 0));

        d_T.add(new PetriT("Обробка", mean));
        d_T.get(0).setDistribution(distributionType, d_T.get(0).getTimeServ());
        d_T.get(0).setParamDeviation(deviation);

        d_In.add(new ArcIn(d_P.get(0), d_T.get(0), 1));
        d_In.add(new ArcIn(d_P.get(2), d_T.get(0), 1));
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(1), 1));
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(2), 1));
        d_Out.add(new ArcOut(d_T.get(0), d_P.get(3), 1));

        ArcIn.initNext();
        PetriP.initNext();
        PetriT.initNext();
        ArcOut.initNext();

        return new PetriNet(identifier, d_P, d_T, d_In, d_Out);
    }
}
