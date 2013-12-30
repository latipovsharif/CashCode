namespace Terminal_Firefox.peripheral {
    public enum CashCodeCommands : byte {
        Reset = 0x30,
        GetStatus = 0x31, 
        SetSecurity = 0x32,
        Poll = 0x33,
        EnableBillTypes = 0x34,
        Stack = 0x35,
        Return = 0x36,
        Identification = 0x37,
        Hold = 0x38,
        SetBarcodeParameters = 0x39,
        ExtractBarcodeData = 0x3A,
        GetBillTable = 0x41,
        GetCrc32OfTheCode = 0x51,
        Download = 0x50,
        RequestStatistics = 0x60,
        AckResponse = 0x00,
        NAckResponse = 0xFF
    }

    public enum CashCodePollResponces : byte {
        PowerUp = 0x10,
        PowerUpWithBillInValidator = 0x11,
        PowerUpWithBillInStacker = 0x12,
        Initialize = 0x13,
        Idling = 0x14,
        Accepting = 0x15,
        Stacking = 0x17,
        Returning = 0x18,
        UnitDisabled = 0x19,
        Holding = 0x1A,
        DeviceBusy = 0x1B,
        GenericRejecting = 0x1C,
        DropCasseteFull = 0x41,
        DropCasseteOutOfPosition = 0x42,
        ValidatorJammed = 0x43,
        DropCasseteJammed = 0x44,
        Cheated = 0x45,
        Pause = 0x46,
        GenericFailure = 0x47,
        BillStacked = 0x81,
        BillReturned = 0x82
    }

    public enum GenericRejectionCodes : byte {
        DueToInsertation = 0x60,
        DueToMagnetic = 0x61,
        DueToRemainedBillInHead = 0x62,
        DueToMultiplying = 0x63,
        DueToConveying = 0x64,
        DueToIdentification = 0x65,
        DueToVerification = 0x66,
        DueToOptic = 0x67,
        DueToInhibit = 0x68,
        DueToCapacity = 0x69,
        DueToOperation = 0x6A,
        DueToLenght = 0x6C
    }

    public enum GenericFailureCodes : byte {
        StackMotor = 0x50,
        TransportMotorSpeed = 0x51,
        TransportMotor = 0x52,
        AligningMotor = 0x53,
        InitialCassetteStatus = 0x54,
        OpticCanal = 0x55,
        MagneticCanal = 0x56,
        CapacitanceCanal = 0x5F
    }

    public enum BillTypes {
        One = 0,
        Three = 1,
        Five = 2,
        Ten = 3,
        Twenty = 4,
        Fifty = 5,
        Hundred = 6,
        TwoHundred = 7,
        FiveHundred = 8
    }
}
