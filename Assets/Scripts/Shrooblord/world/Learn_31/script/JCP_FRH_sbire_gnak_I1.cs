using OpenSpace;
using OpenSpace.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shrooblord.lib;

public class JCP_FRH_sbire_gnak_I1 : MonoBehaviour {
    public PersoBehaviour pirate;
    public SectorComponent sector;
    public MovementComponent mc;

    public SectorManager sectorManager;

    private int initSector = 0;

    private int timer = -1;
    private string state = "asleep";

    //private bool moving = false;
    private Vector3 startPos;
    private Vector3 endPos;

    // Update is called once per frame
    void Update()
    {
        if (pirate == null) {
            foreach (PersoBehaviour pb in FindObjectsOfType<PersoBehaviour>()) {
                string pbName = pb.perso.namePerso;

                if (pbName == "JCP_FRH_sbire_gnak_I1") {
                    //Debug.LogError(pbName);
                    pirate = pb;
                    mc = this.GetComponent<MovementComponent>();
                    if (mc == null)
                        Debug.LogError("Movement Component null!!");
                }
            }
        } else {
            if (!((state == "active") || (state == "dead"))) {
                if (initSector == 0) {
                    //change the active Sector for this Henchman to the area with the climbing tree and the Caterpillar
                    foreach (SectorComponent sc in FindObjectsOfType<SectorComponent>()) {
                        if (sc.name == "Sector @ Learn_31|0x000308F4, SPO @ Learn_31|0x000276F4") {
                            sector = sc;
                            break;
                        }
                    }

                    /* should work but doesn't -- OpenSpace's convenience function
                    sector = sectorManager.GetActiveSectorWrapper(pirate.transform.position);
                    */
                    if (!(sector == null)) {
                        pirate.sector = sector;
                    }

                    //transform
                    pirate.transform.position = new Vector3(-193.61f, 23.84f, 369.45f);
                    pirate.transform.rotation = Quaternion.Euler(0, 0, 0);

                    //walk animation location vectors (see below)
                    startPos = pirate.transform.position;
                    endPos = new Vector3(-183.23f, 23.39f, 364.867f);

                    initSector++;
                } else if (initSector == 1) {
                    //sleeping animation
                    pirate.SetState(48);

                    timer = 90;
                    initSector++;
                }

                if (!(timer == -1)) {
                    if (timer > 0) {
                        timer -= 1;
                    } else {
                        if (state == "asleep") {
                            //wake up and transition to idle
                            pirate.SetState(49);
                            pirate.autoNextState = true;

                            state = "woke up";
                            timer = 40;
                        } else if (state == "woke up") {
                            //be surprised and transition to running
                            pirate.SetState(6);
                            pirate.autoNextState = true;

                            state = "running up";
                            timer = 50;
                        } else if (state == "running up") {
                            pirate.transform.rotation = Quaternion.Euler(0, -45, 0);

                            //lerp move transform to move over level geometry
                            StartCoroutine(mc.moveToLerp(pirate, startPos, endPos, 1.2f));

                            state = "aiming";
                            timer = 75;
                        } else if (state == "aiming") {
                            pirate.transform.rotation = Quaternion.Euler(0, 0, 0);

                            //aim... get ready... (SHOOT!)
                            pirate.SetState(8);
                            pirate.autoNextState = true;

                            state = "active";
                            //inactivate timer
                            timer = -1;
                        }
                    }
                }
            } else if (state == "active") {
                //Henchman is alive and kicking
            } else {
                //Henchman is dead. send flowers
            }
        }
    }
}
