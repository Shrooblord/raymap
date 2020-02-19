using OpenSpace;
using OpenSpace.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JCP_FRH_sbire_gnak_I1 : MonoBehaviour {
    public PersoBehaviour pirate;
    public SectorComponent sector;

    private int initSector = 0;

    // Update is called once per frame
    void Update()
    {
        if (pirate == null) {
            foreach (PersoBehaviour pb in FindObjectsOfType<PersoBehaviour>()) {
                string pbName = pb.perso.namePerso;

                if (pbName == "JCP_FRH_sbire_gnak_I1") {
                    //Debug.LogError(pbName);
                    pirate = pb;
                }
            }
        } else {
            if (initSector == 0) {
                //change the active Sector for this Henchman to the area with the climbing tree and the Caterpillar
                foreach (SectorComponent sc in FindObjectsOfType<SectorComponent>()) {
                    if (sc.name == "Sector @ Learn_31|0x000308F4, SPO @ Learn_31|0x000276F4") {
                        sector = sc;
                        break;
                    }
                }
                if (!(sector == null)) {
                    pirate.sector = sector;
                }

                //transform
                pirate.transform.position = new Vector3(-185, 22, 356);
                pirate.transform.rotation = Quaternion.Euler(0, 0, 0);

                initSector = 1;
            }

            else if (initSector == 1) {
                //more stuff
            }
        }
    }
}
