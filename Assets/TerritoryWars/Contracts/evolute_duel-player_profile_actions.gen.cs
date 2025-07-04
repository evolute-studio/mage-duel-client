// Generated by dojo-bindgen on Mon, 24 Feb 2025 13:57:23 +0000. Do not modify this file manually.
using System;
using System.Threading.Tasks;
using Dojo;
using Dojo.Starknet;
using UnityEngine;
using dojo_bindings;
using System.Collections.Generic;
using System.Linq;
using Enum = Dojo.Starknet.Enum;
using BigInteger = System.Numerics.BigInteger;

// System definitions for `evolute_duel-player_profile_actions` contract
public class Player_profile_actions : MonoBehaviour {
    // The address of this contract
    public string contractAddress;
    
    // Call the `transfer_ownership` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> transfer_ownership(Account account, FieldElement new_owner) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        calldata.Add(new_owner.Inner);

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "transfer_ownership",
                calldata = calldata.ToArray()
            }
        });
    }
            

    
    // Call the `renounce_ownership` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> renounce_ownership(Account account) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "renounce_ownership",
                calldata = calldata.ToArray()
            }
        });
    }
            

    
    // Call the `transferOwnership` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> transferOwnership(Account account, FieldElement newOwner) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        calldata.Add(newOwner.Inner);

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "transferOwnership",
                calldata = calldata.ToArray()
            }
        });
    }
            

    
    // Call the `renounceOwnership` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> renounceOwnership(Account account) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "renounceOwnership",
                calldata = calldata.ToArray()
            }
        });
    }
    
    // Call the `upgrade` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> upgrade(Account account, FieldElement new_class_hash) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        calldata.Add(new_class_hash.Inner);

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "upgrade",
                calldata = calldata.ToArray()
            }
        });
    }
            

    
    // Call the `balance` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> balance(Account account) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "balance",
                calldata = calldata.ToArray()
            }
        });
    }
            

    
    // Call the `username` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> username(Account account) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "username",
                calldata = calldata.ToArray()
            }
        });
    }
            

    
    // Call the `active_skin` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> active_skin(Account account) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "active_skin",
                calldata = calldata.ToArray()
            }
        });
    }
            

    
    // Call the `change_username` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> change_username(Account account, FieldElement new_username) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        calldata.Add(new_username.Inner);

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "change_username",
                calldata = calldata.ToArray()
            }
        });
    }
            

    
    // Call the `become_bot` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> become_bot(Account account) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "become_bot",
                calldata = calldata.ToArray()
            }
        });
    }
    
    
    
    // Call the `change_skin` system with the specified Account and calldata
    // Returns the transaction hash. Use `WaitForTransaction` to wait for the transaction to be confirmed.
    public async Task<FieldElement> change_skin(Account account, byte skin_id) {
        List<dojo.FieldElement> calldata = new List<dojo.FieldElement>();
        calldata.Add(new FieldElement(skin_id).Inner);

        return await account.ExecuteRaw(new dojo.Call[] {
            new dojo.Call{
                to = new FieldElement(contractAddress).Inner,
                selector = "change_skin",
                calldata = calldata.ToArray()
            }
        });
    }
            
}
        