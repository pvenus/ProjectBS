using System.Collections.Generic;
using UnityEngine;

public class PartyControlManager : MonoBehaviour
{
    [Header("Party Members")]
    [SerializeField] private List<PartyMovementMono> members = new();

    [Header("Switch Input")]
    [SerializeField] private bool enableCycleSwitch = false;
    [SerializeField] private KeyCode nextMemberKey = KeyCode.E;
    [SerializeField] private KeyCode previousMemberKey = KeyCode.Q;
    [SerializeField] private KeyCode selectMember1Key = KeyCode.Alpha1;
    [SerializeField] private KeyCode selectMember2Key = KeyCode.Alpha2;
    [SerializeField] private KeyCode selectMember3Key = KeyCode.Alpha3;
    [SerializeField] private bool debugSwitchLog = true;
    [SerializeField] private float switchCooldown = 0.15f;
    private float _lastSwitchTime = -999f;

    [Header("Movement Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";

    private int _currentIndex = -1;
    private PartyMovementMono _currentMember;

    private void Start()
    {
        InitializeMembers();
    }

    private void Update()
    {
        if (members == null || members.Count == 0)
            return;

        HandleSwitchInput();
        HandleMovementInput();
    }

    private void InitializeMembers()
    {
        if (members == null)
            members = new List<PartyMovementMono>();

        members.RemoveAll(member => member == null);

        if (members.Count == 0)
        {
            _currentIndex = -1;
            _currentMember = null;
            return;
        }

        for (int i = 0; i < members.Count; i++)
            members[i].SetMovementControlByPlayer(false);

        SetCurrentMember(0, "initialize");
    }

    private void HandleSwitchInput()
    {
        if (Time.time - _lastSwitchTime < switchCooldown)
            return;

        if (Input.GetKeyDown(selectMember1Key))
        {
            TrySelectMember(0, "input:select-1");
            return;
        }

        if (Input.GetKeyDown(selectMember2Key))
        {
            TrySelectMember(1, "input:select-2");
            return;
        }

        if (Input.GetKeyDown(selectMember3Key))
        {
            TrySelectMember(2, "input:select-3");
            return;
        }

        if (!enableCycleSwitch)
            return;

        if (Input.GetKeyDown(nextMemberKey))
        {
            SwitchToNextMember("input:next");
            return;
        }

        if (Input.GetKeyDown(previousMemberKey))
        {
            SwitchToPreviousMember("input:previous");
        }
    }

    private void HandleMovementInput()
    {
        if (_currentMember == null)
            return;

        float x = Input.GetAxisRaw(horizontalAxis);
        float y = Input.GetAxisRaw(verticalAxis);

        Vector2 moveInput = new Vector2(x, y);
        _currentMember.SetManualMoveInput(moveInput);
    }

    private void TrySelectMember(int index, string reason)
    {
        if (members == null || members.Count == 0)
            return;

        if (index < 0 || index >= members.Count)
            return;

        if (_currentIndex == index)
            return;

        SetCurrentMember(index, reason);
    }

    private void SwitchToNextMember(string reason)
    {
        if (members.Count == 0)
            return;

        int nextIndex = _currentIndex + 1;
        if (nextIndex >= members.Count)
            nextIndex = 0;

        SetCurrentMember(nextIndex, reason);
    }

    private void SwitchToPreviousMember(string reason)
    {
        if (members.Count == 0)
            return;

        int previousIndex = _currentIndex - 1;
        if (previousIndex < 0)
            previousIndex = members.Count - 1;

        SetCurrentMember(previousIndex, reason);
    }

    private void SetCurrentMember(int index, string reason)
    {
        if (members == null || members.Count == 0)
            return;

        if (index < 0 || index >= members.Count)
            return;

        if (_currentMember == members[index] && _currentIndex == index)
            return;

        PartyMovementMono previousMember = _currentMember;
        int previousIndex = _currentIndex;

        if (_currentMember != null)
        {
            _currentMember.SetManualMoveInput(Vector2.zero);
            _currentMember.SetMovementControlByPlayer(false);
        }

        _currentIndex = index;
        _currentMember = members[_currentIndex];
        _lastSwitchTime = Time.time;

        if (_currentMember != null)
        {
            _currentMember.SetMovementControlByPlayer(true);
            _currentMember.SetManualMoveInput(Vector2.zero);
        }

        if (debugSwitchLog)
        {
            Debug.Log(
                $"[PartyControl] Switch reason={reason}, previousIndex={previousIndex}, previousMember={(previousMember != null ? previousMember.name : "null")}, newIndex={_currentIndex}, newMember={(_currentMember != null ? _currentMember.name : "null")}, memberCount={members.Count}"
            );
        }
    }

    public PartyMovementMono GetCurrentMember()
    {
        return _currentMember;
    }

    public int GetCurrentIndex()
    {
        return _currentIndex;
    }

    public void SetMembers(List<PartyMovementMono> newMembers)
    {
        members = newMembers ?? new List<PartyMovementMono>();
        InitializeMembers();
    }

    public void AddMember(PartyMovementMono member)
    {
        if (member == null)
            return;

        if (members == null)
            members = new List<PartyMovementMono>();

        if (members.Contains(member))
            return;

        members.Add(member);
        member.SetMovementControlByPlayer(false);

        if (_currentMember == null)
            SetCurrentMember(0, "add-member-initialize");
    }

    public void RemoveMember(PartyMovementMono member)
    {
        if (member == null || members == null || members.Count == 0)
            return;

        int removeIndex = members.IndexOf(member);
        if (removeIndex < 0)
            return;

        bool wasCurrent = member == _currentMember;

        member.SetManualMoveInput(Vector2.zero);
        member.SetMovementControlByPlayer(false);
        members.RemoveAt(removeIndex);

        if (members.Count == 0)
        {
            _currentIndex = -1;
            _currentMember = null;
            return;
        }

        if (wasCurrent)
        {
            int nextIndex = Mathf.Clamp(removeIndex, 0, members.Count - 1);
            SetCurrentMember(nextIndex, "remove-current-member");
            return;
        }

        if (_currentIndex > removeIndex)
            _currentIndex -= 1;
    }
}
