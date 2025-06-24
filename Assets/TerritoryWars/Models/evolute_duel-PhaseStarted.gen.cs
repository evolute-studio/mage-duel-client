using System;
using Dojo;
using Dojo.Starknet;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Enum = Dojo.Starknet.Enum;
using BigInteger = System.Numerics.BigInteger;

// Model definition for `evolute_duel::eventsModels::EPhaseStarted` model
public class evolute_duel_PhaseStarted : ModelInstance {
    [ModelField("board_id")]
    public FieldElement board_id;

    [ModelField("phase")]
    public byte phase; // 0 - creating, 1 - reveal, 2 - move
    
    [ModelField("top_tile")]
    public Option<byte> top_tile;
    
    [ModelField("commited_tile")]
    public Option<byte> commited_tile;
    
    [ModelField("started_at")]
    public ulong started_at;
    
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() {
    }
}