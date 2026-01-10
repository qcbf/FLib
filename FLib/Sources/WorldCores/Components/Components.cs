// ==================== qcbf@qq.com | 2026-01-07 ====================

using System.Runtime.CompilerServices;

namespace FLib.WorldCores
{

    [SkipLocalsInit]
    public readonly ref struct Components<T1> where T1 : unmanaged
    {
        public readonly Ref<T1> R1;
        
        /// <summary>
        /// 返回组件的字符串表示
        /// </summary>
        public override string ToString() => $"{R1}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1)
        {
            R1 = r1;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2> where T1 : unmanaged where T2 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public override string ToString() => $"{R1}, {R2}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2)
        {
            R1 = r1; R2 = r2;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public override string ToString() => $"{R1}, {R2}, {R3}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3)
        {
            R1 = r1; R2 = r2; R3 = r3;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7, T8> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public readonly Ref<T8> R8;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}, {R8}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7, Ref<T8> r8)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7; R8 = r8;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7, T8, T9> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public readonly Ref<T8> R8;
        public readonly Ref<T9> R9;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}, {R8}, {R9}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7, Ref<T8> r8, Ref<T9> r9)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7; R8 = r8; R9 = r9;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public readonly Ref<T8> R8;
        public readonly Ref<T9> R9;
        public readonly Ref<T10> R10;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}, {R8}, {R9}, {R10}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7, Ref<T8> r8, Ref<T9> r9, Ref<T10> r10)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7; R8 = r8; R9 = r9; R10 = r10;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public readonly Ref<T8> R8;
        public readonly Ref<T9> R9;
        public readonly Ref<T10> R10;
        public readonly Ref<T11> R11;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}, {R8}, {R9}, {R10}, {R11}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7, Ref<T8> r8, Ref<T9> r9, Ref<T10> r10, Ref<T11> r11)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7; R8 = r8; R9 = r9; R10 = r10; R11 = r11;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public readonly Ref<T8> R8;
        public readonly Ref<T9> R9;
        public readonly Ref<T10> R10;
        public readonly Ref<T11> R11;
        public readonly Ref<T12> R12;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}, {R8}, {R9}, {R10}, {R11}, {R12}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7, Ref<T8> r8, Ref<T9> r9, Ref<T10> r10, Ref<T11> r11, Ref<T12> r12)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7; R8 = r8; R9 = r9; R10 = r10; R11 = r11; R12 = r12;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public readonly Ref<T8> R8;
        public readonly Ref<T9> R9;
        public readonly Ref<T10> R10;
        public readonly Ref<T11> R11;
        public readonly Ref<T12> R12;
        public readonly Ref<T13> R13;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}, {R8}, {R9}, {R10}, {R11}, {R12}, {R13}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7, Ref<T8> r8, Ref<T9> r9, Ref<T10> r10, Ref<T11> r11, Ref<T12> r12, Ref<T13> r13)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7; R8 = r8; R9 = r9; R10 = r10; R11 = r11; R12 = r12; R13 = r13;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public readonly Ref<T8> R8;
        public readonly Ref<T9> R9;
        public readonly Ref<T10> R10;
        public readonly Ref<T11> R11;
        public readonly Ref<T12> R12;
        public readonly Ref<T13> R13;
        public readonly Ref<T14> R14;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}, {R8}, {R9}, {R10}, {R11}, {R12}, {R13}, {R14}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7, Ref<T8> r8, Ref<T9> r9, Ref<T10> r10, Ref<T11> r11, Ref<T12> r12, Ref<T13> r13, Ref<T14> r14)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7; R8 = r8; R9 = r9; R10 = r10; R11 = r11; R12 = r12; R13 = r13; R14 = r14;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public readonly Ref<T8> R8;
        public readonly Ref<T9> R9;
        public readonly Ref<T10> R10;
        public readonly Ref<T11> R11;
        public readonly Ref<T12> R12;
        public readonly Ref<T13> R13;
        public readonly Ref<T14> R14;
        public readonly Ref<T15> R15;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}, {R8}, {R9}, {R10}, {R11}, {R12}, {R13}, {R14}, {R15}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7, Ref<T8> r8, Ref<T9> r9, Ref<T10> r10, Ref<T11> r11, Ref<T12> r12, Ref<T13> r13, Ref<T14> r14, Ref<T15> r15)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7; R8 = r8; R9 = r9; R10 = r10; R11 = r11; R12 = r12; R13 = r13; R14 = r14; R15 = r15;
        }
    }

    [SkipLocalsInit]
    public readonly ref struct Components<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged where T4 : unmanaged where T5 : unmanaged where T6 : unmanaged where T7 : unmanaged where T8 : unmanaged where T9 : unmanaged where T10 : unmanaged where T11 : unmanaged where T12 : unmanaged where T13 : unmanaged where T14 : unmanaged where T15 : unmanaged where T16 : unmanaged
    {
        public readonly Ref<T1> R1;
        public readonly Ref<T2> R2;
        public readonly Ref<T3> R3;
        public readonly Ref<T4> R4;
        public readonly Ref<T5> R5;
        public readonly Ref<T6> R6;
        public readonly Ref<T7> R7;
        public readonly Ref<T8> R8;
        public readonly Ref<T9> R9;
        public readonly Ref<T10> R10;
        public readonly Ref<T11> R11;
        public readonly Ref<T12> R12;
        public readonly Ref<T13> R13;
        public readonly Ref<T14> R14;
        public readonly Ref<T15> R15;
        public readonly Ref<T16> R16;
        public override string ToString() => $"{R1}, {R2}, {R3}, {R4}, {R5}, {R6}, {R7}, {R8}, {R9}, {R10}, {R11}, {R12}, {R13}, {R14}, {R15}, {R16}]";

        [SkipLocalsInit]
        public Components(Ref<T1> r1, Ref<T2> r2, Ref<T3> r3, Ref<T4> r4, Ref<T5> r5, Ref<T6> r6, Ref<T7> r7, Ref<T8> r8, Ref<T9> r9, Ref<T10> r10, Ref<T11> r11, Ref<T12> r12, Ref<T13> r13, Ref<T14> r14, Ref<T15> r15, Ref<T16> r16)
        {
            R1 = r1; R2 = r2; R3 = r3; R4 = r4; R5 = r5; R6 = r6; R7 = r7; R8 = r8; R9 = r9; R10 = r10; R11 = r11; R12 = r12; R13 = r13; R14 = r14; R15 = r15; R16 = r16;
        }
    }
}