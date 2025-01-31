using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TM_Player_Movement : MonoBehaviour
{
    Keyboard_Input_Handler input;

    Tilemap player_tm;
    Tilemap room_tm;
    Tilemap boxes_tm;
    Tilemap void_tm;
    Tilemap grab_tm;

    public TileBase pl_up;
    public TileBase pl_down;
    public TileBase pl_left;
    public TileBase pl_right;
    TileBase pl_current;
    public TileBase wall;
    public TileBase void_tile;

    public TileBase ind_up;
    public TileBase ind_down;
    public TileBase ind_left;
    public TileBase ind_right;

    int x = 0;
    int y = 0;

    public bool grab_mode;
    string into_away;
    string side;

    public bool move_enabled = false;


    // Start is called before the first frame update
    void Start()
    {
        input = GameObject.Find("GameRunner").GetComponent<Keyboard_Input_Handler>();
        player_tm = gameObject.GetComponent<Tilemap>();
        room_tm = GameObject.Find("Room").GetComponent<Tilemap>();
        boxes_tm = GameObject.Find("Boxes").GetComponent<Tilemap>();
        void_tm = GameObject.Find("VoidTiles").GetComponent<Tilemap>();
        grab_tm = GameObject.Find("Tilemap Layer Specifically For Grab Effect").GetComponent<Tilemap>();
        player_tm.ClearAllTiles();
        pl_current = pl_down;
        grab_mode = false;
        player_tm.SetTile(new Vector3Int(x, y, 0), pl_current);
    }

    // Update is called once per frame
    void Update()
    {
        if((input.HandleInput() == "up" || input.HandleInput() == "down" || input.HandleInput() == "left" || input.HandleInput() == "right") && !grab_mode && move_enabled) {
            int prev_x = x;
            int prev_y = y;
            player_tm.ClearAllTiles();
            switch(input.HandleInput()) {
                case "up":
                    y += 1;
                    pl_current = pl_up;
                    break;
                case "down":
                    y -=1;
                    pl_current = pl_down;
                    break;
                case "left":
                    x -= 1;
                    pl_current = pl_left;
                    break;
                case "right":
                    x += 1;
                    pl_current = pl_right;
                    break;
            }
            if(room_tm.GetTile(new Vector3Int(x, y, 0)) == wall || void_tm.GetTile(new Vector3Int(x, y, 0)) == void_tile) {
                x = prev_x;
                y = prev_y;
            } else if(boxes_tm.GetTile(new Vector3Int(x, y, 0)) != null) {
                if(!GameObject.Find("Boxes").GetComponent<TM_Box_Move>().AttemptMoveBoxes(new Vector3Int(x, y, 0), input.HandleInput(), boxes_tm.GetTile(new Vector3Int(x, y, 0)))) {
                    x = prev_x;
                    y = prev_y;
                }
            }
            player_tm.SetTile(new Vector3Int(x, y, 0), pl_current);
        } 
        //THIS IS A BAD WAY OF DOING THINGS BUT IM TOO LAZY TO FIX IT
        else if((input.HandleInput() == "up" || input.HandleInput() == "down" || input.HandleInput() == "left" || input.HandleInput() == "right") && grab_mode == true && move_enabled) {
            int prev_x = x;
            int prev_y = y;
            int box_x_offset = 0;
            int box_y_offset = 0;
            bool ignore_input = false;
            into_away = null;
            side = null;
            player_tm.ClearAllTiles();
            if(pl_current == pl_up) {
                side = "bottom";
                switch(input.HandleInput()) {
                    case "up":
                        y += 1;
                        into_away = "into";
                        break;
                    case "down":
                        box_y_offset = 2;
                        y -= 1;
                        into_away = "away";
                        break;
                    default:
                        ignore_input = true;
                        break;
                }
            } else if(pl_current == pl_down) {
                side = "top";
                switch(input.HandleInput()) {
                    case "up":
                        box_y_offset = -2;
                        y += 1;
                        into_away = "away";
                        break;
                    case "down":
                        y -= 1;
                        into_away = "into";
                        break;
                    default:
                        ignore_input = true;
                        break;
                }
            } else if(pl_current == pl_right) {
                side = "left";
                switch(input.HandleInput()) {
                    case "right":
                        x += 1;
                        into_away = "into";
                        break;
                    case "left":
                        box_x_offset = 2;
                        x -= 1;
                        into_away = "away";
                        break;
                    default:
                        ignore_input = true;
                        break;
                }
            } else if(pl_current == pl_left) {
                side = "right";
                switch(input.HandleInput()) {
                    case "right":
                        box_x_offset = -2;
                        x += 1;
                        into_away = "away";
                        break;
                    case "left":
                        x -= 1;
                        into_away = "into";
                        break;
                    default:
                        ignore_input = true;
                        break;
                }
            }
            if(room_tm.GetTile(new Vector3Int(x, y, 0)) == wall || void_tm.GetTile(new Vector3Int(x, y, 0)) == void_tile || (boxes_tm.GetTile(new Vector3Int(x, y, 0)) != null && into_away == "away") || ignore_input == true) {
                x = prev_x;
                y = prev_y;
            } else if(!GameObject.Find("Boxes").GetComponent<TM_Box_Move>().AttemptBoxAlter(
            new Vector3Int(x + box_x_offset, y + box_y_offset, 0), 
            side, 
            into_away,
            boxes_tm.GetTile(new Vector3Int(x + box_x_offset, y + box_y_offset, 0)))) {
                x = prev_x;
                y = prev_y;
            }
            player_tm.SetTile(new Vector3Int(x, y, 0), pl_current);
        }

        if(input.HandleInput() == "grab" && move_enabled) {
            Vector3Int in_front = new Vector3Int(x, y, 0);
            if(pl_current == pl_up) {
                in_front += new Vector3Int(0, 1, 0);
            } else if(pl_current == pl_down) {
                in_front += new Vector3Int(0, -1, 0);
            } else if(pl_current == pl_right) {
                in_front += new Vector3Int(1, 0, 0);
            } else if(pl_current == pl_left) {
                in_front += new Vector3Int(-1, 0, 0);
            }
            if(boxes_tm.GetTile(in_front) != null) {
                switch(grab_mode) {
                    case true:
                        grab_mode = false;
                        Debug.Log("grab disabled");
                        break;
                    case false:
                        grab_mode = true;
                        Debug.Log("grab enabled");
                        break;
                }
            }
        }

        grab_tm.ClearAllTiles();
        if(grab_mode) {
            if(pl_current == pl_up) {
                grab_tm.SetTile(new Vector3Int(x, y, 0), ind_up);
            } else if(pl_current == pl_down) {
                grab_tm.SetTile(new Vector3Int(x, y, 0), ind_down);
            } else if(pl_current == pl_left) {
                grab_tm.SetTile(new Vector3Int(x, y, 0), ind_left);
            } else if(pl_current == pl_right) {
                grab_tm.SetTile(new Vector3Int(x, y, 0), ind_right);
            }
        }
    }

    public void Reload(int new_x, int new_y) {
        grab_mode = false;
        pl_current = pl_down;
        x = new_x;
        y = new_y;
        player_tm.ClearAllTiles();
        player_tm.SetTile(new Vector3Int(x, y, 0), pl_current);
    }
}
