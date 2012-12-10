#include "pch.h"
#include "Node.h"
#include <time.h>


const float Node::Speed = 0.6f;
const float Node::AngleAdd = 0.1f;
float Node::MaxConnectedness = 0.1f;

Node::Node(XMFLOAT2 screenSize)
{
    m_position = XMFLOAT2(200,200);
    m_size = 50;
    m_screenSize = screenSize;
    m_position = XMFLOAT2(
        NodeSizeMax + (random(screenSize.x - 2*NodeSizeMax)),
        NodeSizeMax + (random(screenSize.y - 2*NodeSizeMax)));
	m_newPosition = XMFLOAT2(
        NodeSizeMax + (random(screenSize.x - 2*NodeSizeMax)),
        NodeSizeMax + (random(screenSize.y - 2*NodeSizeMax)));
    m_connectedness = 0;
    m_normalisedConnectedness = 0;
    m_color = Colors::White;
    m_id = -1;
}

void Node::Update(float timeTotal, float timeDelta)
{
    if(m_id == -1)
    {
		if(random(1000) > 999)
        {
            m_newPosition = XMFLOAT2(
				NodeSizeMax + (random(m_screenSize.x - 2*NodeSizeMax)),
				NodeSizeMax + (random(m_screenSize.y - 2*NodeSizeMax)));
        }
    }

	if (abs(m_position.x - m_newPosition.x) > 0.5f && abs(m_position.y - m_newPosition.y) > 0.5f)
    {
        // the inertia calculation. Only move the ellipse a fraction of the distance in the direction of the lead node
        m_position.x += (float)((m_newPosition.x - m_position.x) * Speed * timeDelta);
        m_position.y += (float)((m_newPosition.y - m_position.y) * Speed * timeDelta);
    }

	m_destRect.left =   (long)m_position.x;
	m_destRect.top =    (long)m_position.y;
	m_destRect.right  = (long)(m_position.x + m_size);  
	m_destRect.bottom = (long)(m_position.y + m_size);
}

void Node::ApplyConnection(float connectedness, Node* node2)
{
    // increase the connectedness
    m_connectedness += connectedness;

    // this allows us to get a reliable value for MaxConnectedness. Used for Mapping the Connectedness value
    if (m_connectedness > MaxConnectedness)
        MaxConnectedness = m_connectedness;

    // create a normalised version of the Connectedness variable
    m_normalisedConnectedness = Map(m_connectedness, 0, MaxConnectedness, 0, 1);
}

void Node::FinishConnection()
{
    // calculate the node size from NormalisedConnectedness
    m_size = Map(m_normalisedConnectedness, 0, 1, (float)NodeSizeMin, (float)NodeSizeMax);

    m_connectedness = 0;
}

const float Node::Map(float value,float start1,float end1,float start2,float end2)
{
    return (start2 + ((value - start1) / (end1 - start1) * (end2 - start2)));
}

XMVECTOR Node::GetPosition()
{
    return XMLoadFloat2(&m_position);
}

XMFLOAT2 Node::GetPositionF()
{
    return m_position;
}

void Node::SetPosition(float x, float y)
{
    m_newPosition = XMFLOAT2(x,y);
}

int Node::GetId()
{
    return m_id;
}

void Node::SetId(int id)
{
    m_id = id;
}

int Node::SetUniqueId()
{
	return m_id = (int)(time(NULL));
}