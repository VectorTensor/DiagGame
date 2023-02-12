using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Web;
using System.Text.RegularExpressions;
using System.Linq;

using System;
using System.Net.Http.Headers;
using UnityEngine.EventSystems;
//using static System.Net.Mime.MediaTypeNames;
// Since action doesnt take parameters we need to make private variables that act as parameters for the action delegate

public class Controller2 : MonoBehaviour
{
    // Request json classes 
  
    [System.Serializable]
     public class SecondRequest{
        public int npc;
        public int pc;
    }
     // Response json classes
    [System.Serializable]
     public class SecondResponse{
         public string Message;
         public int action;
     }
    [System.Serializable]
    public class ActionResponse{
        public int transformed_action;
    }

    [System.Serializable]
    public class  ThirdRequest{
        public int reward;
      
    }

        [System.Serializable]
    public class ActionRequest{
        public int Superstate;
        public int action;
    }

   
    // Prefab for button
    public GameObject button;

    // wait for input
    private bool waitforinput;
    // Conversation runnning
    private bool conversation;
    //Talk button
    GameObject Talk;
    //Conversation elements
    GameObject ConversationElement;
    //Each episode data
    private List<int> next_state = new List<int>();
    private int action;
    private int trans_action;
    private int choice;
    private int reward;
    private bool is_done;
    private int episode =1;
    
    private List<string> buttonsAdded = new List<string>();
    private int n_Superstate;
    private int Superstate ; 
    private List<int> stateDialogueIndex; // Stores the int of index that are the dialogues for this state
    private List<string> stateCharacter; 
    private string PlayerName = "Witcher";
    private string NPC_name ="Chader"; 
    private List<int> state = new List<int>();
    static readonly HttpClient client = new HttpClient();
    private List<string> chad_dialogue = new List<string>(){
        "hello witcher",
            "hi",
        "hey kid!",
        "How is it going on brave witcher lord?",
        "Witcher can you save us from the creature thats attacking us?",
        "Oe witcher, defeat a monster for me.",
        "Lord witcher can you save us from the creature thats attacking us?",
        "We don't know what monster attacked us but it was vicious. It must have to huge as it slaughtered no less than a dozen wolves. Ripped their guts out, but left lost uneaten. Howls at nighhts. People are afraid to venture into the woods at night. Please lord witcher save us from the beast.",
        "We don't know what monster attacked us but it was vicious. It must have to huge as it slaughtered no less than a dozen wolves. Ripped their guts out, but left lost uneaten. Howls at nighhts. People are afraid to venture into the woods at night. Please lord witcher save us from the beast.",
        "No idea what monster attacked us if i knew i would be dead. It must have to huge as it slaughtered no less than a dozen wolves. Ripped their guts out, but left lost uneaten. You sure kid like you can handle it. Howls at nighhts. People are afraid to venture into the woods at night. Sure you can take care of it we dont want any more dead bodies."
    };
    private List<string> pc_dialogue = new List<string>(){
        "Greetings",
        "Fine just busy fighting creatures from Void dimension",
        "Ok let me help you with it. What creature is it?",
        " I m gonna help you but i need to paid well for a contract like that. What kind of creature is it",
        "I don't have to bother with this work. farewell",
        " Ok, sir let me help you. What kind of creature is it.",
        "Sure it will take 500$ ",
        " Sure it will take 800$",
        "Sure it will take 1200$",
        "Sorry I am busy. I won't be able to do that."


    };
    // Reward List 
    List<int> pc_reward = new List<int>() {1,3,3,1,-10,2,2,5,-5,-10 };
    List<int> npc_reward = new List<int>() { 0, -1, -2, -3, -1, 0, 2, -3, -1, 1 };
    private Text dialogue;
    private Text name;
            // Start is called before the first frame update
    void Start()
    {
        // Initialize some stuffs
        Talk = GameObject.Find("Talk");
        Button talk_button = Talk.GetComponent<Button>();
        talk_button.onClick.AddListener(beginConvo);
        ConversationElement = GameObject.Find("Conversation");

        ConversationElement.SetActive(false);
        //beginConvo();




    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space") && conversation)
        {   if (!waitforinput){
            stateChange();

        }
            //print("space key was pressed");
        }
    }
    void beginConvo()
    {

        ConversationElement.SetActive(true);
        Talk.SetActive(false);
        conversation = true;
        dialogue = GameObject.Find("dialoguetext").GetComponent<Text>();
        name = GameObject.Find("name").GetComponent<Text>();
        Superstate = 0;
        next_state.Insert(0, action);
        next_state.Insert(1, choice);
        state = next_state;
        stateChange();
        // Activate UI elements
       


    }
    void endConversation()
    {
        conversation=false;
        GameObject gm = GameObject.Find("Conversation");
        episode++;
        ConversationElement.SetActive(false);
        Talk.SetActive(true);  
    }
    
    void DisplayChoices(){
                waitforinput = true;

            GameObject canvas = GameObject.Find("Canvas");
            
            List<int> dialogueindex = new List<int>();

            switch(Superstate){
                case 1: 
                dialogueindex= new List<int>(){0};
                break;
                case 3: 
                dialogueindex =new List<int>() {1};
                break;
                case 5:
                dialogueindex =new List<int>() {5};
                break;
                case 6:
                dialogueindex =new List<int>() {2,3,4} ;
                break;
                case 9:
                dialogueindex =new List<int>() {6,7,8,9};
                break;
                default:
                Debug.Log("Problem occured at showing the choices");
                break;


            }
            int y =2;
            int length =  dialogueindex.Count;
            for (int i=0 ; i< length; i++){
                GameObject gm = Instantiate(button, new Vector3(0, y, 0), Quaternion.identity,canvas.transform);
                gm.name = "choice"+ dialogueindex[i].ToString();
                Text dialogue_choice = gm.transform.Find("Text").GetComponent<Text>();
                dialogue_choice.text = pc_dialogue[dialogueindex[i]];
                y=y-1;
                Button bt = gm.GetComponent<Button>();
                bt.onClick.AddListener(buttonClick);
                buttonsAdded.Add(gm.name);
                

            }

    }
    public async void buttonClick(){
        Debug.Log(EventSystem.current.currentSelectedGameObject.name);
        string chosedButton = (string) EventSystem.current.currentSelectedGameObject.name;
        string name_Button = GameObject.Find(chosedButton).name;
        int length = buttonsAdded.Count;
        string resultString = new String(name_Button.Where(Char.IsDigit).ToArray());
        choice = Int32.Parse(resultString);
        for (int i=0; i<length;i++){
            GameObject gm = GameObject.Find(buttonsAdded[i]);
            Destroy(gm);

        }
        waitforinput = false;

        // Display the PC dialogue
        dialogue.text = pc_dialogue[choice]; 
        name.text = PlayerName;

        // get the reward 

        reward = pc_reward[choice] + npc_reward[trans_action];

        //choice = 4;
        // get the reward 
        //reward = 10; //just for api testing purpose
        //Put this code in the button click handler 
        next_state.Insert(0,action);
        next_state.Insert(1,choice);

        if (Superstate == 6 && action == 2 ){
            is_done = true;



        }
        else if (Superstate == 9 ){
            is_done = true;
        }
        else{
            is_done = false;
        }
      

        // send state, action ,reward , next_state, is_done to the end point.

        ThirdRequest tr = new ThirdRequest();
        
        tr.reward = reward; 
        

        string data = JsonUtility.ToJson(tr);
        string content =await ApiCall("http://127.0.0.1:8000/policy/train/",data);
        if (is_done){
            await GetApiCall("http://127.0.0.1:8000/policy/train/");
        }
        NextSuperstate();
       

    }
    void NextSuperstate()
    {
        if (Superstate == 0)
        {
            Superstate = 1;
        }
        else if (Superstate == 1)
        {
            Superstate = 2;
        }
        else if (Superstate == 2)
        {
            if (action == 0)
            {

                Superstate = 3;
            }
            else
            {
                Superstate = 6;

            }

        }
        else if (Superstate == 3)
        {
            Superstate = 4;

        }
        else if (Superstate == 4)
        {
            Superstate = 5;
        }
        else if (Superstate == 5)
        {
            Superstate = 8;

        }

        else if (Superstate == 6)
        {
            if (choice == 4)
            {
                Superstate = 7;
            }
            else
            {
                Superstate = 8;

            }
        }
        else if (Superstate == 7)
        {
            Superstate = 7;

        }
        else if(Superstate == 8)
        {
            Superstate = 9;
        }
        else if(Superstate == 9)
        {
            Superstate = 7;

        }
        else
        {
            Debug.Log("Problem accured ");
        }
    }
    async Task<string> ApiCall(string url1, string data){
        
        UriBuilder builder = new UriBuilder(url1);
        //Debug.Log(apiurl);
       // FirstRequest fr = new FirstRequest();
       // fr.episode = episode; 
       // string data = JsonUtility.ToJson(fr);
        var buffer = System.Text.Encoding.UTF8.GetBytes(data);
    var byteContent = new ByteArrayContent(buffer);
    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

       // builder.Query = "episode=1";
        var url = builder.ToString();
        var res = await client.PostAsync(url,byteContent);
        var content = await res.Content.ReadAsStringAsync();
        
       // Debug.Log(content);
       
        return content;
        }        
    async Task GetApiCall(string url1){
        UriBuilder builder = new UriBuilder(url1);
        var url = builder.ToString();
        var res = await client.GetAsync(url);
        var content = await res.Content.ReadAsStringAsync();
        Debug.Log(content);

    }
    async Task DialogueProcess(int episode){
        // Call api
        
        // First iniitalize model 
        
        await GetApiCall("http://127.0.0.1:8000/policy/");

        

        // send the current state
        SecondRequest sr = new SecondRequest();
       // Debug.Log("before");
        sr.npc = state[0];
        sr.pc = state[1];

        string data1 = JsonUtility.ToJson(sr); 
        string content1 = await ApiCall("http://127.0.0.1:8000/policy/",data1);
        SecondResponse srs = JsonUtility.FromJson<SecondResponse>(content1);

        action = srs.action;

        // Transform the action
        ActionRequest ar = new ActionRequest();
        
        ar.Superstate = Superstate;
        ar.action = action;
        string data = JsonUtility.ToJson(ar);
        
        string content = await ApiCall("http://127.0.0.1:8000/dqn/action/", data);
        ActionResponse ars = JsonUtility.FromJson<ActionResponse>(content);
        trans_action = ars.transformed_action;
        // view the npc dialogue
        dialogue.text = chad_dialogue[trans_action];
        name.text = NPC_name;
        NextSuperstate();



    }
    async Task train(){


        // ask the pc dialogue 
        
        DisplayChoices();
        //while(waitforinput);
       

    }
    void DisplayDialogue(){
        // Code to display dialogue according to the state
    }
    async void stateChange(){
        // State machine code
        state = next_state;
        switch(Superstate){
            case 0 :
                //stateDialogueIndex = new List<int>();
                // call the output functions here 
                await DialogueProcess(episode);

              //  Superstate = 1;
               // stateDialogueIndex.Add(0);
                //stateCharacter.Add(PlayerName);
                break;
            case 1 :
                await train();
              //  Superstate = 2;

                

                break;
            case 2: 
                
                await DialogueProcess(episode);
                
            break;
            case 3:
            await train();
          //  Superstate = 4;
            break;

            case 4:
            await DialogueProcess(episode);
            Superstate = 5; 
            break;
            case 5:
            await train();
         //   Superstate = 8;
            break;
            case 6:
            await train();
                
           

            break;
            case 7: 
            // stop dialogue
            endConversation();  
            break;
            case 8: 
            await DialogueProcess(episode);
          //  Superstate = 9 ;
            break;
            case 9: 
            await train();
          //  Superstate = 7;
            break;
            default:
            Debug.Log("there is a problem");
            break;
            



        
        }
    }
}







































