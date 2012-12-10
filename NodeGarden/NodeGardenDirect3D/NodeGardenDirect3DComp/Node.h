#pragma once

#include "Sprite.h"
#include <math.h>
#include <string.h>

using namespace Microsoft::WRL;
using namespace DirectX;

#define random(x) (((float)rand()/(float)RAND_MAX)*(x))
#define PI 3.1415926f
#define PIOVER2 1.5707963f
#define MinDist 250.0f          // minimum distance between 2 nodes for a connection

class Node : public Sprite
{
public:
    Node() {};
    Node(XMFLOAT2 screenSize);

    ~Node(void) {};

    virtual void Update(float timeTotal, float timeDelta);
    virtual void ApplyConnection(float connectedness, Node* node2);
    virtual void FinishConnection();
    XMVECTOR GetPosition();
    XMFLOAT2 GetPositionF();
    void SetPosition(float x, float y);
    int GetId();
    void SetId(int id);
	int SetUniqueId();

    static const float Map(float value,float start1,float end1,float start2,float end2);

protected:
    static const float Speed;
    static const float AngleAdd;             // the degree to which the node changes direction.
    static const int NodeSizeMin = 20;       // the Connectedness value determins the node size. This number is between nodeSizeMin and nodeSizeMax
    static const int NodeSizeMax = 50;
    static float MaxConnectedness;

    XMFLOAT2 m_screenSize;
    XMFLOAT2 m_position;
	XMFLOAT2 m_newPosition;
    float m_size;
    float m_normalisedConnectedness;
    int m_id;

private:
    float m_connectedness;
};

