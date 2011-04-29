﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Subscriptions
{
	using System;
	using System.Collections.Generic;
	using SubscriptionBuilders;

	/// <summary>
	/// Manages the subscription and un subscription of message consumers to the
	/// service bus as part of the bus lifecycle.
	/// 
	/// As a bus service, once the bus is started and operational, this service 
	/// will get started. At this point, any registered consumers and sagas will
	/// be subscribed to the bus. 
	/// 
	/// When stop is called, those subscriptions will be removed unless the
	/// registration information indicated that the subscription is meant to be
	/// persistent, and not removed on service shutdown.
	/// </summary>
	public class SubscriptionBusService :
		IBusService
	{
		IList<SubscriptionBuilder> _builders;
		IList<ISubscriptionReference> _subscriptions;

		public SubscriptionBusService(IList<SubscriptionBuilder> builders)
		{
			_builders = builders;

			_subscriptions = new List<ISubscriptionReference>();
		}

		bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~SubscriptionBusService()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				_builders.Clear();
				_subscriptions.Clear();
			}

			_disposed = true;
		}
		public void Start(IServiceBus bus)
		{
			foreach (SubscriptionBuilder builder in _builders)
			{
				try
				{
					ISubscriptionReference subscription = builder.Subscribe(bus);
					_subscriptions.Add(subscription);
				}
				catch (Exception)
				{
					StopAllSubscriptions();
					throw;
				}
			}
		}

		public void Stop()
		{
			StopAllSubscriptions();
		}

		void StopAllSubscriptions()
		{
			_subscriptions.Each(x => x.OnStop());
			_subscriptions.Clear();
		}
	}
}