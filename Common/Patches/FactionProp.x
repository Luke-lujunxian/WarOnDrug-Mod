<?xml version="1.0" encoding="utf-8" ?>
<Patch>
    <Operation Class="PatchOperationConditional">
        <xpath>/Defs/ThingsDef[defName="Pawn"]/comps</xpath>
        <nomatch Class="PatchOperationAdd">
            <xpath>/Defs/ThingsDef[defName="Pawn"]</xpath>
            <value>
                <comps>
                    <li>
                        <compClass>WarOnDrug.WodFactionLeaderPropertiesComp</compClass>
                    </li>
                </comps>
            </value>
        </nomatch>
        <match Class="PatchOperationAdd">
            <xpath>/Defs/ThingsDef[defName="Pawn"]/comps</xpath>
            <value>
                <li>
                    <compClass>WarOnDrug.WodFactionLeaderPropertiesComp</compClass>
                </li>
            </value>
        </match>
    </Operation>
</Patch>