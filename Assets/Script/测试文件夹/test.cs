using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using proto.BattleMsg;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    public InputField textInput;

    public InputField idInput;

    public InputField pwInput;

    void Start()
    {
        NetManager.AddEventListener(NetEvent.ConnectSucc, OnConnectSucc);

        NetManager.AddEventListener(NetEvent.ConnectFial, OnConnectFail);

        NetManager.AddEventListener(NetEvent.Close, OnClose);

        NetManager.AddMsgListeners("MsgMove", OnMsgMove);

        NetManager.AddMsgListeners("MsgPing", OnMsgPing);

        NetManager.AddMsgListeners("MsgRegister", OnMsgRegister);

        NetManager.AddMsgListeners("MsgLogin", OnMsgLogin);

        NetManager.AddMsgListeners("MsgGetText", OnMsgGetText);

        NetManager.AddMsgListeners("MsgSaveText", OnMsgSaveText);

        NetManager.AddMsgListeners("MsgKick", OnMsgKick);
    }
    
    public void OnMsgKick(MsgBase msgBase)
    {
        Debug.Log("��������");
    }

    public void OnMsgGetText(MsgBase msgBase)
    {
        MsgGetText msg = (MsgGetText)msgBase;

        textInput.text = msg.text;

        Debug.Log("��ȡ�ı��ɹ�");
    }

    //������水ť
    public void OnMsgSaveClick()
    {
        MsgSaveText msg = new MsgSaveText();

        msg.text = textInput.text;

        NetManager.Send(msg);
    }

    public void OnMsgSaveText(MsgBase msgBase)
    {
        MsgSaveText msg = (MsgSaveText)msgBase;

        if(msg.result == 0)
        {
            Debug.Log("����ɹ�");
        }
        else
        {
            Debug.Log("����ʧ��");
        }
    }
    
    //���Ӻ���
    public void OnConnectClick()
    {
        NetManager.Connect("127.0.0.1", 8888);
    }


    //����Ͽ����԰�ť
    public void OnCloseClick()
    {
        NetManager.Close();
    }



    //��¼����
    public void OnLoginClick()
    {
        MsgLogin msgLogin = new MsgLogin();

        msgLogin.id = idInput.text;

        msgLogin.pw = pwInput.text;

        NetManager.Send(msgLogin);
    }

    public void OnMsgLogin(MsgBase msgBase)
    {
        MsgLogin msg = (MsgLogin)msgBase;

        if(msg.result == 0)
        {
            Debug.Log("��¼�ɹ�");

            MsgGetText msgTextMsg = new MsgGetText();

            NetManager.Send(msgTextMsg);
        }
        else
        {
            Debug.Log("��¼ʧ��");
        }
    }

    //ע�ᰴť
    public void OnMsgRegisterClick()
    {
        MsgRegister msg = new MsgRegister();

        if(idInput.text == null || pwInput.text == null)
        {
            Debug.Log("�˺ź����붼����Ϊ��");

            return;
        }

        msg.id = idInput.text;

        msg.pw = pwInput.text;

        NetManager.Send(msg);
    }


    public void OnMsgRegister(MsgBase msgBase)
    {
        MsgRegister msg = (MsgRegister)msgBase;

        if(msg.result == 0)
        {
            Debug.Log("ע��ɹ�");
        }
        else
        {
            Debug.Log("ע��ʧ��");
        }
    }

    public void OnMsgPing(MsgBase msgBase)
    {
        Debug.Log("MsgPing");
    }


    public void OnMsgMove(MsgBase msgBase)
    {
        MsgMove msgMove = (MsgMove)msgBase;

        Debug.Log("OnMsgMove: " + msgMove.x);
    }


    public void OnConnectSucc(string err)
    {
        Debug.Log("���ӳɹ�");

        //MsgMove msgMove = new MsgMove();

        //msgMove.x = 15;

        //msgMove.y = 20;

        //msgMove.z = 45;

        //MsgPing msgPing = new MsgPing();

        //NetManager.Send(msgMove);

        //NetManager.Send(msgPing);
    }

    public void OnClose(string err)
    {
        Debug.Log("�ر�����");
    }

    public void OnConnectFail(string err)
    {
        Debug.Log("����ʧ��");
    }
    
    void Update()
    {
        NetManager.Update();
    }
}
