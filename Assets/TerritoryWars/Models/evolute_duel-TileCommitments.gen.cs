// Generated by dojo-bindgen on Tue, 10 Jun 2025 15:27:25 +0000. Do not modify this file manually.
using System;
using Dojo;
using Dojo.Starknet;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Enum = Dojo.Starknet.Enum;
using BigInteger = System.Numerics.BigInteger;


// Model definition for `evolute_duel::models::game::TileCommitments` model
public class evolute_duel_TileCommitments : ModelInstance {
    [ModelField("board_id")]
    public FieldElement board_id;

    [ModelField("player")]
    public FieldElement player;

    [ModelField("tile_commitments")]
    public FieldElement[] tile_commitments;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }
}