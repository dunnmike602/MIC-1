namespace MLDComputing.Emulators.MIC1.Core.Bus;

public enum ALUOperation
{
    Nop = -1,
    PassA = 0,        
    PassB = 1,     
    APlusB = 2,       
    AMinusB = 3,     
    BMinusA = 4,      
    IncrementA = 5,       
    IncrementB = 6,     
    DecrementA = 7,      
    DecrementB = 8,    
    NegateA = 9,     
    NegateB = 10,   
    Zero = 11,        
    One = 12,          
    IncrementBBy4 = 13,
    DecrementBBy4 = 14,
    CombineOffset = 15,
    CombineHighLow = 16,
    SignExtend8 = 17,
    IncrementBBy2 = 18,
    IncrementBBy3 = 19,
    DecrementBBy2 = 20,
}