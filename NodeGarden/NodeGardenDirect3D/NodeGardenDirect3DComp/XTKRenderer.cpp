#include "pch.h"

#include "XTKRenderer.h"

using namespace Microsoft::WRL;
using namespace Windows::Foundation;
using namespace Windows::UI::Core;


XTKRenderer::XTKRenderer()
{
    srand((unsigned)time(0));
    m_isLoaded = false;
}

void XTKRenderer::CreateDeviceResources()
{
    Direct3DBase::CreateDeviceResources();

    m_pSpriteBatch = std::unique_ptr<SpriteBatch>(new SpriteBatch(m_d3dContext.Get()));
}

void XTKRenderer::ChangeNodeAmount(int newAmount)
{
    NodeNum = newAmount;
    LineNum = (((NodeNum)*((NodeNum)-1))/2);

    m_nodes.resize(NodeNum);
    m_lines.resize(LineNum);

    for (int i = 1; i < NodeNum; i++)
    {
        m_nodes[i] = new ShadowNode(XMFLOAT2(m_renderTargetSize.Width, m_renderTargetSize.Height));
    }
    
    for (int i = 0; i < LineNum; i++)
    {
        m_lines[i] = new LineConnection();
    }

    m_isLoaded = true;
}

Windows::Foundation::Point XTKRenderer::CreateMyNode()
{
    m_nodes.clear();
    m_nodes.push_back(new MyNode(XMFLOAT2(m_renderTargetSize.Width, m_renderTargetSize.Height)));

    return GetMyNodePosition();
}

void XTKRenderer::CreateWindowSizeDependentResources()
{
    Direct3DBase::CreateWindowSizeDependentResources();

    CreateDDSTextureFromFile(m_d3dDevice.Get(), L"node.DDS", nullptr, &m_pTexture );

    // blend state description - alpha blend
    D3D11_BLEND_DESC bDesc;
    ZeroMemory(&bDesc, sizeof(D3D11_BLEND_DESC) );

    bDesc.AlphaToCoverageEnable = false;
    bDesc.IndependentBlendEnable = false;        
    bDesc.RenderTarget[0].BlendEnable = true;
    bDesc.RenderTarget[0].SrcBlend = D3D11_BLEND_SRC_ALPHA;
    bDesc.RenderTarget[0].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
    bDesc.RenderTarget[0].BlendOp = D3D11_BLEND_OP_ADD;
    bDesc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_SRC_ALPHA;      
    bDesc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_DEST_ALPHA; 
    bDesc.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_ADD;
    bDesc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL ;

    m_d3dDevice->CreateBlendState( &bDesc, &m_pBlendState );           // create a  new blend state
}

void XTKRenderer::OnPointerPressed(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args)
{
    m_nodes[0]->OnPointerPressed(XMFLOAT2(args->CurrentPoint->Position.X, args->CurrentPoint->Position.Y));
}

void XTKRenderer::OnPointerMoved(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args)
{
    m_nodes[0]->OnPointerMoved(XMFLOAT2(args->CurrentPoint->Position.X, args->CurrentPoint->Position.Y));
}

void XTKRenderer::OnPointerReleased(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args)
{
    m_nodes[0]->OnPointerReleased();
}

Windows::Foundation::Point XTKRenderer::GetMyNodePosition()
{
    XMFLOAT2 pos = m_nodes[0]->GetPositionF();
    return Windows::Foundation::Point(pos.x, pos.y);
}

float Distance(const XMVECTOR& vector1,const XMVECTOR& vector2)
{
    XMVECTOR vectorSub = XMVectorSubtract(vector1,vector2);
    XMVECTOR length = XMVector3Length(vectorSub);

    float distance = 0.0f;
    XMStoreFloat(&distance,length);
    return distance;
}

void XTKRenderer::Update(float timeTotal, float timeDelta)
{
    int currentLine = 0;
    for (int i = 0; i < NodeNum; i++)
    {
        m_nodes[i]->Update(timeTotal, timeDelta);
        
        float distance, connectedness;
        for (int j = i + 1; j != NodeNum; ++j)
        {
            // calculate the distance between each 2 nodes
            distance = Distance(m_nodes[i]->GetPosition(), m_nodes[j]->GetPosition());

            // if distance is within the threshold
            if (distance < MinDist)
            {
                // add a mapped value between 1-0 to each node's connectedness value
                connectedness = Node::Map(distance, 0, MinDist, 1, 0);
                m_nodes[i]->ApplyConnection(connectedness, m_nodes[j]);
                m_nodes[j]->ApplyConnection(connectedness, m_nodes[i]);

                m_lines[currentLine]->FormConnection(m_nodes[i]->GetPosition(), m_nodes[j]->GetPosition(), distance);
            }
            else
            {
                m_lines[currentLine]->BreakConnection();
            }

            // switch the currently used line in the pool
            currentLine = (currentLine + 1) % LineNum;
        }

        m_nodes[i]->FinishConnection();
    }
}

// clear screen to light grey
const float bgColor[] = { 0.1f, 0.1f, 0.1f, 1.0f };

void XTKRenderer::Render()
{
    m_d3dContext->ClearRenderTargetView(m_renderTargetView.Get(), bgColor);
    m_d3dContext->OMSetRenderTargets(1, m_renderTargetView.GetAddressOf(), NULL);

    // begin the spritebatch using the alpha blend state
    m_pSpriteBatch->Begin(SpriteSortMode_BackToFront, m_pBlendState.Get());
    for (int i = 0; i < NodeNum; i++)
    {
        m_nodes[i]->DrawSprites(m_pSpriteBatch.get(), m_pTexture.Get());
    }

    for (int i = 0; i < LineNum; i++)
    {
        m_lines[i]->DrawSprites(m_pSpriteBatch.get(), m_pTexture.Get());
    }
    m_pSpriteBatch->End();

}

XTKRenderer::~XTKRenderer()
{
    for (unsigned int i = 0; i < m_nodes.size(); i++)
    {
        delete m_nodes[i];
        m_nodes[i] = NULL;
    }
    m_nodes.clear();

    for (unsigned int i = 0; i < m_lines.size(); i++)
    {
        delete m_lines[i];
        m_lines[i] = NULL;
    }
    m_lines.clear();
}

void XTKRenderer::UpdateNodePosition(int nodeId, float nodeX, float nodeY)
{
    for (int i = 1; i < m_nodes.size(); i++)
    {
        if(m_nodes[i]->GetId() == nodeId)
        {
            m_nodes[i]->SetPosition(nodeX, nodeY);
            return;
        }
    }

    for (int i = 1; i < m_nodes.size(); i++)
    {
        if(m_nodes[i]->GetId() == -1)
        {
            m_nodes[i]->SetId(nodeId);
            m_nodes[i]->SetPosition(nodeX, nodeY);
			return;
        }
    }

	CreateNode(nodeX, nodeY);
}

bool XTKRenderer::IsLoaded()
{
    return m_isLoaded;
}

int XTKRenderer::CreateNode(float nodeX, float nodeY)
{
	m_nodes.push_back(new ShadowNode(XMFLOAT2(m_renderTargetSize.Width, m_renderTargetSize.Height), XMFLOAT2(nodeX, nodeY)));

	int oldLineNum = LineNum;
	NodeNum = m_nodes.size();
    LineNum = (((NodeNum)*((NodeNum)-1))/2);

	for (int i = 0; i < LineNum - oldLineNum; i++)
	{
		m_lines.push_back(new LineConnection());
	}

	return m_nodes[m_nodes.size() - 1]->SetUniqueId();
}

void XTKRenderer::RemoveNode(int nativeId)
{
	for (std::vector<ShadowNode*>::iterator it = m_nodes.begin(); it != m_nodes.end(); )
	{
		if( (*it)->GetId() == nativeId )
		{
			if((*it)->GetId() == m_nodes[0]->GetId())
				return;

			delete * it;
			it = m_nodes.erase(it);
			int oldLineNum = LineNum;
			NodeNum = m_nodes.size();
			LineNum = (((NodeNum)*((NodeNum)-1))/2);
			m_lines.resize(LineNum);
		}
		else 
		{
			++it;
		}
	}
}


