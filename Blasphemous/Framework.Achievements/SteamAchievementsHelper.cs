using System;
using Steamworks;
using UnityEngine;

namespace Framework.Achievements;

public class SteamAchievementsHelper : IAchievementsHelper
{
	private bool m_bStoreStats;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private CGameID m_GameID;

	private Callback<UserStatsReceived_t> m_UserStatsReceived;

	private Callback<UserStatsStored_t> m_UserStatsStored;

	private Callback<UserAchievementStored_t> m_UserAchievementStored;

	private bool steamInitialized;

	private bool isOnline;

	public SteamAchievementsHelper()
	{
		steamInit();
	}

	private void steamInit()
	{
		if (SteamManager.Initialized)
		{
			m_GameID = new CGameID(SteamUtils.GetAppID());
			m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
			steamInitialized = true;
			if (!SteamUserStats.RequestCurrentStats())
			{
				Debug.LogError("RequestCurrentStats returned false!");
			}
		}
		else
		{
			Debug.LogError("SteamManager not Initialized!");
		}
	}

	public void SetAchievementProgress(string Id, float value)
	{
		if (isOnline)
		{
			if (m_bStatsValid)
			{
				try
				{
					if (!(value < 100f))
					{
						SteamUserStats.SetAchievement(Id);
						if (!SteamUserStats.StoreStats())
						{
							Debug.LogError("SetAchievementProgress: we couldn't store the stats!");
						}
					}
					return;
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.Message);
					return;
				}
			}
			Debug.LogError("SetAchievementProgress: stats aren't valid!");
		}
		else
		{
			Debug.LogError("SetAchievementProgress: we are not online!");
		}
	}

	public void GetAchievementProgress(string Id, GetAchievementOperationEvent evt)
	{
		if (isOnline)
		{
			if (m_bStatsValid)
			{
				try
				{
					bool pbAchieved = false;
					SteamUserStats.GetAchievement(Id, out pbAchieved);
					if (pbAchieved)
					{
						evt(Id, 100f);
					}
					else
					{
						evt(Id, 0f);
					}
					return;
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.Message);
					return;
				}
			}
			Debug.LogError("GetAchievementProgress: stats aren't valid!");
		}
		else
		{
			Debug.LogError("GetAchievementProgress: we are not online!");
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_nMaxProgress == 0)
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
				return;
			}
			Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
		}
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (SteamManager.Initialized && (ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				Debug.Log("Received stats and achievements from Steam");
				m_bStatsValid = true;
				isOnline = true;
			}
			else
			{
				Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				Debug.Log("StoreStats - success");
			}
			else if (pCallback.m_eResult == EResult.k_EResultInvalidParam)
			{
				Debug.Log("StoreStats - some failed to validate");
				UserStatsReceived_t pCallback2 = default(UserStatsReceived_t);
				pCallback2.m_eResult = EResult.k_EResultOK;
				pCallback2.m_nGameID = (ulong)m_GameID;
				OnUserStatsReceived(pCallback2);
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}
}
