namespace DemContainer {
    public interface IConstructor {
        void Construct();
    }

    public interface IConstructor<T1> {
        void Construct(T1 t1);
    }
    
    public interface IConstructor<T1, T2> {
        void Construct(T1 t1, T2 t2);
    }
    
    public interface IConstructor<T1, T2, T3> {
        void Construct(T1 t1, T2 t2, T3 t3);
    }
    
    public interface IConstructor<T1, T2, T3, T4> {
        void Construct(T1 t1, T2 t2, T3 t3, T4 t4);
    }
    
    public interface IConstructor<T1, T2, T3, T4, T5> {
        void Construct(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
    }
    
    public interface IConstructor<T1, T2, T3, T4, T5, T6> {
        void Construct(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
    }
    
    public interface IConstructor<T1, T2, T3, T4, T5, T6, T7> {
        void Construct(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
    }
    
    public interface IConstructor<T1, T2, T3, T4, T5, T6, T7, T8> {
        void Construct(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
    }
    
    public interface IConstructor<T1, T2, T3, T4, T5, T6, T7, T8, T9> {
        void Construct(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9);
    }
    
    public interface IConstructor<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> {
        void Construct(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10);
    }
}