using UnityEngine;
using UnityEngine.AI;

public class PolitistPatrol : MonoBehaviour
{
    public enum StareAI { Patrulare, Urmarire }

    [Header("Status Curent (Read Only)")]
    public StareAI stareCurenta = StareAI.Patrulare;

    [Header("Setari Patrulare")]
    public Transform[] punctePatrulare;
    public float distantaOprire = 1f;
    public bool patrulareAleatoare = false;

    [Header("Setari Urmarire")]
    public Transform tinta;
    public float distantaDetectie = 5f;
    public float distantaRenuntare = 15f;
    public float vitezaUrmarire = 5f;
    public float vitezaPatrulare = 2f;

    [Header("Rotire Smooth")]
    public float vitezaRotire = 5f;

    [Header("Vizibilitate")]
    public float unghiVizibilitate = 120f;
    public LayerMask layerObstacol;

    // Componente si variabile interne
    private NavMeshAgent agent;
    private Animator animator;
    private int indexPunctCurent = 0;
    private int ultimulIndexPunct = -1;

    // Timer pentru deblocare (daca se blocheaza in pereti)
    private float timpBlocaj = 0f;
    private float timeoutBlocaj = 3f;

    // Variabila pentru a tine evidenta daca am anuntat muzica
    private bool amAnuntatCaUrmaresc = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (animator != null) animator.applyRootMotion = false;

        // 1. NAVMESH: Setare sa mearga peste tot (NavMesh fix)
        agent.areaMask = NavMesh.AllAreas;
        agent.speed = vitezaPatrulare;

        // 2. TINTA: Cautare automata player
        if (tinta == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) tinta = player.transform;
        }

        // Pornire patrulare
        if (punctePatrulare.Length > 0)
        {
            indexPunctCurent = GasestePunctulCelMaiApropriat();
            MergeLaPunct(indexPunctCurent);
        }
    }

    void Update()
    {
        // Rotire vizuala lina a modelului
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion rotatieTinta = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotatieTinta, Time.deltaTime * vitezaRotire);
        }

        VerificaSchimbareStare();

        if (stareCurenta == StareAI.Urmarire)
            ComportamentUrmarire();
        else
            ComportamentPatrulare();
    }

    void VerificaSchimbareStare()
    {
        if (tinta == null) return;

        float distanta = Vector3.Distance(transform.position, tinta.position);

        // --- TRANZITIE SPRE URMARIRE ---
        if (stareCurenta == StareAI.Patrulare)
        {
            if (distanta <= distantaDetectie && PoateVeziTinta())
            {
                stareCurenta = StareAI.Urmarire;
                agent.speed = vitezaUrmarire;

                // Porneste Muzica (anunta managerul)
                if (!amAnuntatCaUrmaresc && ManagerMuzica.instanta != null)
                {
                    ManagerMuzica.instanta.PolitistAInceputUrmarirea();
                    amAnuntatCaUrmaresc = true;
                }
            }
        }
        // --- TRANZITIE SPRE PATRULARE ---
        else if (stareCurenta == StareAI.Urmarire)
        {
            if (distanta > distantaRenuntare || !PoateVeziTinta())
            {
                stareCurenta = StareAI.Patrulare;
                agent.speed = vitezaPatrulare;

                // Gaseste cel mai apropiat punct de patrulare si mergi la el
                indexPunctCurent = GasestePunctulCelMaiApropriat();
                MergeLaPunct(indexPunctCurent);

                // Opreste/Scade Muzica (anunta managerul)
                if (amAnuntatCaUrmaresc && ManagerMuzica.instanta != null)
                {
                    ManagerMuzica.instanta.PolitistATerminatUrmarirea();
                    amAnuntatCaUrmaresc = false;
                }
            }

            // Verificare distanta pentru Game Over (Backup daca nu merge coliziunea)
            if (distanta < 1.0f)
            {
                TeAPrins();
            }
        }
    }

    bool PoateVeziTinta()
    {
        if (tinta == null) return false;
        Vector3 directie = (tinta.position - transform.position).normalized;
        float distanta = Vector3.Distance(transform.position, tinta.position);

        // Verifica unghiul
        if (Vector3.Angle(transform.forward, directie) > unghiVizibilitate / 2f)
            return false;

        // Verifica obstacolele (Raycast de la nivelul ochilor)
        if (Physics.Raycast(transform.position + Vector3.up * 1.6f, directie, distanta, layerObstacol))
            return false;

        return true;
    }

    void ComportamentUrmarire()
    {
        if (tinta != null)
            agent.SetDestination(tinta.position);
    }

    void ComportamentPatrulare()
    {
        // Daca am ajuns la destinatie
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            int vechiIndex = indexPunctCurent;

            if (patrulareAleatoare)
            {
                int nouIndex;
                do { nouIndex = Random.Range(0, punctePatrulare.Length); }
                while (punctePatrulare.Length > 1 && nouIndex == ultimulIndexPunct);
                indexPunctCurent = nouIndex;
            }
            else
            {
                indexPunctCurent = (indexPunctCurent + 1) % punctePatrulare.Length;
            }

            ultimulIndexPunct = vechiIndex;
            MergeLaPunct(indexPunctCurent);
        }

        // Logic anti-blocare
        if (agent.velocity.sqrMagnitude < 0.01f) timpBlocaj += Time.deltaTime;
        else timpBlocaj = 0f;

        if (timpBlocaj > timeoutBlocaj)
        {
            indexPunctCurent = (indexPunctCurent + 1) % punctePatrulare.Length;
            MergeLaPunct(indexPunctCurent);
            timpBlocaj = 0f;
        }
    }

    int GasestePunctulCelMaiApropriat()
    {
        int indexCelMaiApropriat = 0;
        float distantaMin = float.MaxValue;
        for (int i = 0; i < punctePatrulare.Length; i++)
        {
            if (punctePatrulare[i] != null)
            {
                float dist = Vector3.Distance(transform.position, punctePatrulare[i].position);
                if (dist < distantaMin)
                {
                    distantaMin = dist;
                    indexCelMaiApropriat = i;
                }
            }
        }
        return indexCelMaiApropriat;
    }

    void MergeLaPunct(int index)
    {
        if (punctePatrulare == null || punctePatrulare.Length == 0) return;
        if (index < 0 || index >= punctePatrulare.Length || punctePatrulare[index] == null) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(punctePatrulare[index].position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.isStopped = false;
        }
    }

    // --- GAME OVER LOGIC ---
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Declanseaza Game Over doar daca il urmareste activ
            if (stareCurenta == StareAI.Urmarire)
            {
                TeAPrins();
            }
        }
    }

    void TeAPrins()
    {
        // 1. Oprim muzica de urmarire
        if (ManagerMuzica.instanta != null)
        {
            ManagerMuzica.instanta.PolitistATerminatUrmarirea();
        }

        // 2. Declansam Game Over
        if (GameOverManager.instanta != null)
        {
            GameOverManager.instanta.DeclanseazaGameOver();
        }
        else
        {
            Debug.LogError("Lipseste scriptul GameOverManager din scena!");
        }
    }

    // IMPORTANT: Pentru sistemul de muzica (Safety Check)
    void OnDestroy()
    {
        if (amAnuntatCaUrmaresc && ManagerMuzica.instanta != null)
        {
            ManagerMuzica.instanta.PolitistATerminatUrmarirea();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distantaDetectie);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distantaRenuntare);
    }
}